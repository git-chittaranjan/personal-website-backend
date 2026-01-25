using my_api_app.Responses;

namespace my_api_app.Exceptions
{
    public sealed class ValidationException : BusinessException
    {
        public ValidationException(object errors) : base(Statuses.ValidationFailed, errors)
        {
        }
    }
}
