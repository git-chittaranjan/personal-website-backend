using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.ServerExceptions
{
    public class SmtpServiceUnavailableException : BusinessException
    {
        public SmtpServiceUnavailableException() : base(Statuses.SmtpServiceUnavailable)
        { }
    }
}
