using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.UserExceptions
{
    public sealed class UserNotFoundException :BusinessException
    {
        public UserNotFoundException() : base(Statuses.UserNotFound)
        {
        }
    }
}
