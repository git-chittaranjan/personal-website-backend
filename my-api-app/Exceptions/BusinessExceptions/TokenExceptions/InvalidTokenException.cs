using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.TokenExceptions
{
    public sealed class InvalidTokenException : BusinessException
    {
        public InvalidTokenException() : base(Statuses.InvalidToken) 
        { }
    }
}
