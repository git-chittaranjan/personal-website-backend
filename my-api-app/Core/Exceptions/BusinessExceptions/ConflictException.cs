using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions
{
    public sealed class ConflictException : BusinessException
    {
        public ConflictException(ApiStatus? status = null) : base(status ?? Statuses.UserAlreadyExists)
        {
        }
    }
}