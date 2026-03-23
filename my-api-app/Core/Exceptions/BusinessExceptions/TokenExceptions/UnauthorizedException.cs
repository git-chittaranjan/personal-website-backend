using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.TokenExceptions
{
    public sealed class UnauthorizedException : BusinessException
    {
        public UnauthorizedException(ApiStatus? status = null) : base(status ?? Statuses.Unauthorized) 
        {
        }
    }
}
