using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.OtpExceptions
{
    public sealed class OtpExpiredException : BusinessException
    {
        public OtpExpiredException() : base(Statuses.OtpExpired) 
        { }
    }
}
