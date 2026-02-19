using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.ServerExceptions
{
    public class ServiceUnavailableException : BusinessException
    {
        public ServiceUnavailableException() : base(Statuses.ServiceUnavailable) 
        { }
    }
}
