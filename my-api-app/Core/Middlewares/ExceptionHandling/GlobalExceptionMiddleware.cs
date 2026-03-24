using Serilog.Context;
using System.Security.Claims;
using System.Text.Json;
using System;
using my_api_app.Core.Responses;
using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Helpers;

namespace my_api_app.Core.Middlewares.ExceptionHandling
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
            var prodError = new[] {
                new
                {
                    code = "INTERNAL_ERROR",
                    message = "An unexpected error occurred. Please contact support with the trace_id."
                }
            };
            var businessError = new[] {
                new
                {
                    code = "BUSINESS_ERROR",
                    message = "The request could not be processed due to business rules."
                }
            };

            switch (exception)
            {
                // 1a. FOR CONFIGURATION (APPSETTINGS.JSON) EXCEPTIONS OR DOMAIN EXCEPTIONS
                case ConfigurationException ce:  // checked first, catches only ConfigurationException
                    status = ce.Status;
                    errorObject = _env.IsProduction() ? prodError : ce.Errors;
                    break;

                // 1b. FOR BUSINESS EXCEPTIONS OR DOMAIN EXCEPTIONS
                case BusinessException be:
                    status = be.Status; //Accesses Status property defined in the BusinessException class and assigns variable status.
                    errorObject = _env.IsProduction() ? businessError : be.Errors;
                    break;

                // 2. FOR FLUENT VALIDATION EXCEPTIONS - Never Executing, instead ModelValidationResponseExtension.cs is executing
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
                    errorObject = _env.IsProduction() ? prodError : new { detail = ane.Message, parameter = ane.ParamName };
                    break;

                // 3b. ARGUMENT/INVALID OPERATION
                case ArgumentException ae:
                    status = Statuses.BadRequest;
                    errorObject = _env.IsProduction() ? businessError : new { detail = ae.Message, parameter = ae.ParamName };
                    break;

                // 4. FALLBACK: ANY OTHER SYSTEM/UNHANDLED EXCEPTION
                default:
                    status = Statuses.InternalServerError;
                    errorObject = _env.IsProduction() ? prodError : new { detail = exception.Message, stack_trace = exception.StackTrace, type = exception.GetType().Name };
                    break;
            }

            // Structured error log — ALWAYS log stack trace in production for 5xx
            using (LogContext.PushProperty("EventType", "UnhandledException"))
            using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
            {
                if (status.HttpCode >= 500)
                    _logger.LogError(exception,
                        "Unhandled exception | ErrorCode:{ErrorCode} | ErrorMessage:{ErrorMessage} | Path:{Path} | User:{UserId}",
                        status.StatusCode, status.Message, context.Request.Path, userId);
                else //This will only capture 4XX because 2XX & 3XX are Success so they will never hit this Middleware
                    _logger.LogWarning(
                        "Client error | ErrorCode:{ErrorCode} | ErrorMessage:{ErrorMessage} | Path:{Path} | User:{UserId}",
                        status.StatusCode, status.Message, context.Request.Path, userId);
            }

            context.Response.StatusCode = status.HttpCode;

            var response = _responseFactory.Failure(status, errorObject);

            var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}