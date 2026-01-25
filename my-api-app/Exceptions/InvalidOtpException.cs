using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class InvalidOtpException : BusinessException
    {
        public InvalidOtpException() : base(Statuses.InvalidOtp) 
        { }
    }
}
