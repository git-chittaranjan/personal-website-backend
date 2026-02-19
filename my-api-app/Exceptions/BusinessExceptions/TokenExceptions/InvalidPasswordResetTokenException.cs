using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.TokenExceptions
{
    public class InvalidPasswordResetTokenException : BusinessException
    {
        public InvalidPasswordResetTokenException() : base(Statuses.InvalidPasswordResetToken)
        { }
    }
}
