using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.ServerExceptions
{
    public class InternalServerException : BusinessException
    {
        public InternalServerException() : base(Statuses.InternalServerError) 
        { }
    }
}
