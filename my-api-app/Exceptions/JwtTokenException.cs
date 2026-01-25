using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class JwtTokenException : BusinessException
    {
        public JwtTokenException(ApiStatus status) : base(status)
        { }
    }
}
