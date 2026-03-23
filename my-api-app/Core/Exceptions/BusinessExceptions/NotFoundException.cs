using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(ApiStatus? status = null) : base(status ?? Statuses.ResourceNotFound)
        {
        }
    }
}
