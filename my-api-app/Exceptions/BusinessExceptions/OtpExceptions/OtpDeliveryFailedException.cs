using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.OtpExceptions
{
    public sealed class OtpDeliveryFailedException : BusinessException
    {
        public OtpDeliveryFailedException() : base(Statuses.OtpDeliveryFailed)
        {}
    }
}
