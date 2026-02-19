using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.UserExceptions
{
    public class UserAlreadyExistsException : BusinessException
    {
        public UserAlreadyExistsException() : base(Statuses.UserAlreadyExists) 
        { }
    }
}
