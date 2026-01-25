using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public abstract class BusinessException : Exception
    {
        public ApiStatus Status { get; }
        public object? Errors { get; }

        protected BusinessException(ApiStatus status, object? errors = null)
            : base(status.Message)
        {
            Status = status;
            Errors = errors;
        }
    }
}
