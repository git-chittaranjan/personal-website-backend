using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class OtpExpiredException : BusinessException
    {
        public OtpExpiredException() : base(Statuses.OtpExpired) 
        { }
    }
}
