using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions
{
    public sealed class TooManyRequestsException : BusinessException
    {
        public TooManyRequestsException() : base(Statuses.TooManyRequests) 
        { }
    }
}
