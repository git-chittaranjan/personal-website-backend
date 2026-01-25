using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class TooManyRequestsException : BusinessException
    {
        public TooManyRequestsException() : base(Statuses.TooManyRequests) 
        { }
    }
}
