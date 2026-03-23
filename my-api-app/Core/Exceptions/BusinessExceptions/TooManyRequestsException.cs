using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions
{
    public sealed class TooManyRequestsException : BusinessException
    {
        public TooManyRequestsException() : base(Statuses.TooManyRequests) 
        { }
    }
}
