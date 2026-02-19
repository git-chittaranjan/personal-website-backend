using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.UserExceptions
{
    public class PendingUserNotFoundException : BusinessException
    {
        public PendingUserNotFoundException() : base(Statuses.PendingUserNotFound) 
        { }
    }
}
