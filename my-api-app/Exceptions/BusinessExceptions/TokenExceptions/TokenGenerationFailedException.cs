using my_api_app.Responses;

namespace my_api_app.Exceptions.BusinessExceptions.TokenExceptions
{
    public sealed class TokenGenerationFailedException : BusinessException
    {
        public TokenGenerationFailedException() : base(Statuses.TokenGenerationFailed)
        {
        }
    }
}
