using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.UserExceptions
{
    public sealed class InvalidUserIdException : BusinessException
    {
        public InvalidUserIdException() : base(Statuses.InvalidUserId)
        { }
    }
}
