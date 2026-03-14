using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.UserExceptions
{
    public sealed class UserDeletionFailedException : BusinessException
    {
        public UserDeletionFailedException() : base(Statuses.UserDeletionFailed)
        { }
    }
}
