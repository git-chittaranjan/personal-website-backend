using my_api_app.Responses;
using System.Net;
using System.Text.Json;

namespace my_api_app.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        //private readonly RequestDelegate _next;

        //public GlobalExceptionMiddleware(RequestDelegate next)
        //{
        //    _next = next;
        //}

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    try
        //    {
        //        await _next(context);
        //    }
        //    catch (Exception)
        //    {
        //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //        context.Response.ContentType = "application/json";

        //        var response = new ApiResponse<object>
        //        {
        //            Success = false,
        //            Code = Statuses.InternalError.Code,
        //            Message = Statuses.InternalError.Message
        //        };

        //        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        //    }
        //}
    }
}
