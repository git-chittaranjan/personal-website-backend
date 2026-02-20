
using FluentValidation;
using my_api_app.DTOs;
using my_api_app.Exceptions.BusinessExceptions;
using my_api_app.Helpers;
using my_api_app.Responses;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    // Cached to avoid recreating on every exception
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy()
    };


    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        context.Response.ContentType = "application/json";

        ApiStatus status;
        object? errorObject = null;
        var traceId = context.TraceIdentifier;

        switch (exception)
        {
            // 1. FOR BUSINESS EXCEPTIONS OR DOMAIN EXCEPTIONS
            case BusinessException be:
                status = be.Status; //Accesses Status property defined in the BusinessException class and assigns variable status.
                errorObject = be.Errors;
                logger.LogWarning(exception, "Business exception occurred");
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
                logger.LogWarning(exception, "Validation exception occurred");
                break;

            // 3. ARGUMENT/INVALID OPERATION
            case ArgumentException ae:
                status = Statuses.BadRequest;
                errorObject = new[] { ae.Message };
                logger.LogWarning(exception, "Argument exception occurred");
                break;

            // 4. FALLBACK: ANY OTHER SYSTEM/UNHANDLED EXCEPTION
            default:
                status = Statuses.InternalServerError;
                logger.LogError(exception, "Unhandled system exception occurred");
                break;
        }

        context.Response.StatusCode = status.HttpCode;

        var response = ApiResponseFactory.Failure(status, errorObject);

        var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
