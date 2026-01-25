using my_api_app.Exceptions;
using my_api_app.Responses;
using System.Text.Json;

public sealed class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ApiResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.HasStarted)
                return;

            if (context.Response.StatusCode == StatusCodes.Status204NoContent)
                return;
        }
        catch (BusinessException ex)
        {
            await WriteErrorResponse(context, ex.Status, ex.Errors);
            return;
        }
        catch (Exception)
        {
            await WriteErrorResponse(context, Statuses.InternalError, null);
            return;
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, ApiStatus status, object? errors)
    {
        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status.HttpCode;

        var response = new ApiResponse<object>
        {
            StatusCode = status.HttpCode,
            Description = status.Message,
            Errors = errors is null ? null : JsonSerializer.Serialize(errors),
            Total = 1,
            Page = 1
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response)
        );
    }
}
