using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class ConflictException : BusinessException
    {
        public ConflictException(ApiStatus? status = null) : base(status ?? Statuses.UserAlreadyExists)
        {
        }
    }
}
