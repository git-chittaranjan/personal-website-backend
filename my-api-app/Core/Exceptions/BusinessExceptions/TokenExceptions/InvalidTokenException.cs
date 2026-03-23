using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.TokenExceptions
{
    public sealed class InvalidTokenException : BusinessException
    {
        public InvalidTokenException() : base(Statuses.InvalidToken) 
        { }
    }
}
