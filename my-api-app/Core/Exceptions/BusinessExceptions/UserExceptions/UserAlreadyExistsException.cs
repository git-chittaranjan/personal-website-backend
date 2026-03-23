using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.UserExceptions
{
    public class UserAlreadyExistsException : BusinessException
    {
        public UserAlreadyExistsException() : base(Statuses.UserAlreadyExists) 
        { }
    }
}
