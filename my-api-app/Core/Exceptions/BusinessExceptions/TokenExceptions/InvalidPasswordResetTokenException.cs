using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.TokenExceptions
{
    public class InvalidPasswordResetTokenException : BusinessException
    {
        public InvalidPasswordResetTokenException() : base(Statuses.InvalidPasswordResetToken)
        { }
    }
}
