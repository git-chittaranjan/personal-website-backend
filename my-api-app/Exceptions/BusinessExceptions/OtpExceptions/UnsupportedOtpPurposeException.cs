using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.OtpExceptions
{
    public class UnsupportedOtpPurposeException : BusinessException
    {
        public UnsupportedOtpPurposeException() : base(Statuses.UnsupportedOtpPurpose) 
        { }
    }
}
