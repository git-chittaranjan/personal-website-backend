using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions
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
