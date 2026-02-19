using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(ApiStatus? status = null) : base(status ?? Statuses.ResourceNotFound)
        {
        }
    }
}
