using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions
{
    public sealed class ConflictException : BusinessException
    {
        public ConflictException(ApiStatus? status = null) : base(status ?? Statuses.UserAlreadyExists)
        {
        }
    }
}