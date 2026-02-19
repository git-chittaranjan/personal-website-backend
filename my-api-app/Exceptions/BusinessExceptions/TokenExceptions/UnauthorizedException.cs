using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.TokenExceptions
{
    public sealed class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(ApiStatus? status = null) : base(status ?? Statuses.Unauthorized) 
        {
        }
    }
}
