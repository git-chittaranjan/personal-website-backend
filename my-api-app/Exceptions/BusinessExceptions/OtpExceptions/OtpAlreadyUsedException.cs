using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.OtpExceptions
{
    public sealed class OtpAlreadyUsedException : BusinessException
    {
        public OtpAlreadyUsedException() : base(Statuses.OtpAlreadyUsed) 
        { }
    }
}
