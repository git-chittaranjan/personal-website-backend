using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class InvalidCredentialsException : BusinessException
    {
        public InvalidCredentialsException() : base(Statuses.InvalidCredentials)
        { }
    }
}
