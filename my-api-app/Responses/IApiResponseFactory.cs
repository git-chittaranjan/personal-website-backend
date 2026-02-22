namespace my_api_app.Responses
{
    public interface IApiResponseFactory
    {
        ApiResponse<object> Success(ApiStatus status);
        ApiResponse<T> Success<T>(ApiStatus status, T data, Pagination? pagination = null);
        ApiResponse<object> Failure(ApiStatus status, object? errors = null);
    }
}
