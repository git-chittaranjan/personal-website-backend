using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.UserExceptions
{
    public sealed class InvalidCredentialsException : BusinessException
    {
        public InvalidCredentialsException() : base(Statuses.InvalidCredentials)
        { }
    }
}
