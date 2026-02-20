using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_api_app.Responses;
using System.Text.Json;

namespace my_api_app.Helpers
{
    public static class ApiResponseFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };


        //IHttpContextAccessor s a wrapper that allows to access HttpContext from anywhere outside Controllers and Middlewares
        //Controllers and middleware have direct access to HttpContext, but services, helpers, and other classes do not
        private static IHttpContextAccessor? _httpContextAccessor;

        // Called once at startup
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        //Evaluated on every times method is called giving the correct TraceId for each request.
        private static string? GetTraceId()
        {
            return _httpContextAccessor?.HttpContext?.TraceIdentifier;
        }


        // Success response without data
        public static ApiResponse<object> Success(ApiStatus status)
        {
            return new ApiResponse<object>
            {
                Success = true,
                StatusCode = status.StatusCode,
                Description = status.Message,
                Data = null,
                Errors = null,
                Pagination = new Pagination(),
                TraceId = GetTraceId(),
            };
        }

        // Success response with data
        public static ApiResponse<T> Success<T>(ApiStatus status, T data, Pagination? pagination = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = status.StatusCode,
                Description = status.Message,
                Data = data,
                Errors = null,
                Pagination = pagination,
                TraceId = GetTraceId(), 
            };
        }

        // Error response
        public static ApiResponse<object> Failure(ApiStatus status, object? errors = null)
        {
            return new ApiResponse<object>
            {
                Success = false,
                StatusCode = status.StatusCode,
                Description = status.Message,
                Data = null,
                Errors = errors,
                Pagination = null,
                TraceId = GetTraceId()
            };
        }
    }
}
