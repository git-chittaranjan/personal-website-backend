using my_api_app.Helpers;
using System.Text.Json;

namespace my_api_app.Responses
{
    public class ApiResponseFactory : IApiResponseFactory
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };


        //IHttpContextAccessor s a wrapper that allows to access HttpContext from anywhere outside Controllers and Middlewares
        //Controllers and middleware have direct access to HttpContext, but services, helpers, and other classes do not
        private readonly IHttpContextAccessor _context;

        public ApiResponseFactory(IHttpContextAccessor context)
        {
            _context = context;
        }
        
        //Evaluated on every times method is called giving the correct TraceId for each request.
        private string? GetTraceId()
        {
            return _context?.HttpContext?.TraceIdentifier;
        }


        // Success response without data
        public ApiResponse<object> Success(ApiStatus status)
        {
            return new ApiResponse<object>
            {
                Success = true,
                StatusCode = status.StatusCode,
                Description = status.Message,
                Data = null,
                Errors = null,
                Pagination = null,
                TraceId = GetTraceId(),
            };
        }

        // Success response with data
        public ApiResponse<T> Success<T>(ApiStatus status, T data, Pagination? pagination = null)
        {
            ArgumentNullException.ThrowIfNull(data); // Fail fast if null is passed accidentally

            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = status.StatusCode,
                Description = status.Message,
                Data = data,
                Errors = null,
                Pagination = new Pagination(),
                TraceId = GetTraceId(),
            };
        }

        // Error response: Use this to returnn failure without throwing an exception
        public ApiResponse<object> Failure(ApiStatus status, object? errors = null)
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
