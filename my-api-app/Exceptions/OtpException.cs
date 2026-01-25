using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class OtpException : BusinessException
    {
        public OtpException(ApiStatus status) : base(status) 
        { }
    }
}
