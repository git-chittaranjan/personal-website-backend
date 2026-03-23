using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.OtpExceptions
{
    public sealed class OtpException : BusinessException
    {
        public OtpException(ApiStatus status) : base(status) 
        { }
    }
}
