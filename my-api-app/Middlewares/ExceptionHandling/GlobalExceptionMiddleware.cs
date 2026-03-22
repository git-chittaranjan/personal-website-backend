using my_api_app.DTOs;
using my_api_app.Exceptions.BusinessExceptions;
using my_api_app.Helpers;
using my_api_app.Responses;
using Serilog.Context;
using System.Security.Claims;
using System.Text.Json;
using System;

namespace my_api_app.Middlewares.ExceptionHandling
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiResponseFactory _responseFactory;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;


        // Cached to avoid recreating on every exception
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };


        public GlobalExceptionMiddleware(RequestDelegate next, IApiResponseFactory responseFactory, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _responseFactory = responseFactory;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (System.Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, System.Exception exception)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            context.Response.ContentType = "application/json";

            ApiStatus status;
            object? errorObject = null;

            switch (exception)
            {
                // 1. FOR BUSINESS EXCEPTIONS OR DOMAIN EXCEPTIONS
                case BusinessException be:
                    status = be.Status; //Accesses Status property defined in the BusinessException class and assigns variable status.
                    errorObject = be.Errors;
                    break;

                // 2. FOR FLUENT VALIDATION EXCEPTIONS - Not getting called, so added ModelValidationResponseExtension.cs clss
                case FluentValidation.ValidationException ve:
                    status = Statuses.ValidationFailed;
                    errorObject = ve.Errors
                        .Select(e => new ValidationErrorDto
                        {
                            Field = e.PropertyName,
                            Error = e.ErrorMessage
                        }).ToList();
                    break;

                // 3a. PROGRAMMING ERRORS — is of Internal Server Error type
                case ArgumentNullException ane:
                    status = Statuses.InternalServerError;
                    errorObject = _env.IsProduction() ? null : new { detail = ane.Message, parameter = ane.ParamName };
                    break;

                // 3b. ARGUMENT/INVALID OPERATION
                case ArgumentException ae:
                    status = Statuses.BadRequest;
                    errorObject = _env.IsProduction() ? null : new { detail = ae.Message, parameter = ae.ParamName };
                    break;

                // 4. FALLBACK: ANY OTHER SYSTEM/UNHANDLED EXCEPTION
                default:
                    status = Statuses.InternalServerError;
                    errorObject = _env.IsProduction() ? null : new { detail = exception.Message, stack_trace = exception.StackTrace, type = exception.GetType().Name };
                    break;
            }

            // Structured error log — ALWAYS log stack trace in production for 5xx
            using (LogContext.PushProperty("EventType", "UnhandledException"))
            using (LogContext.PushProperty("ErrorCode", status.StatusCode))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
            {
                if (status.HttpCode >= 500)
                    _logger.LogError(exception,
                        "Unhandled exception | ErrorCode:{ErrorCode} | Path:{Path} | User:{UserId}",
                        status.StatusCode, context.Request.Path, userId);
                else //This will only capture 4XX because 2XX & 3XX are Success so they will never hit this Middleware
                    _logger.LogWarning(
                        "Client error | ErrorCode:{ErrorCode} | Path:{Path} | User:{UserId}",
                        status.StatusCode, context.Request.Path, userId);
            }

            context.Response.StatusCode = status.HttpCode;

            var response = _responseFactory.Failure(status, errorObject);

            var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}