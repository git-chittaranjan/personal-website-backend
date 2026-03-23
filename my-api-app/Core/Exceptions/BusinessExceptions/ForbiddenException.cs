using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions
{
    public sealed class ForbiddenException : BusinessException
    {
        public ForbiddenException() : base(Statuses.Forbidden) 
        {
        }
    }
}
