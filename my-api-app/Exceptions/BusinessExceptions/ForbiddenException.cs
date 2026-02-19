using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions
{
    public sealed class ForbiddenException : BusinessException
    {
        public ForbiddenException() : base(Statuses.Forbidden) 
        {
        }
    }
}
