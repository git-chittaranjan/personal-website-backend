using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class TokenExpiredException : BusinessException
    {
        public TokenExpiredException() : base(Statuses.TokenExpired) 
        { }
    }
}
