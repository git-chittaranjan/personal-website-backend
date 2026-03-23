using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;

namespace my_api_app.Core.Exceptions.BusinessExceptions.ServerExceptions
{
    public class ServiceUnavailableException : BusinessException
    {
        public ServiceUnavailableException() : base(Statuses.ServiceUnavailable) 
        { }
    }
}
