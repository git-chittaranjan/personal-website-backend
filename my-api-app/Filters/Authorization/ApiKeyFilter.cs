using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using my_api_app.Responses;
using System.Security.Cryptography;
using System.Text;

namespace my_api_app.Filters.Authorization
{
    public class ApiKeyFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _config;
        private readonly IApiResponseFactory _responseFactory;

        public ApiKeyFilter(IConfiguration config, IApiResponseFactory responseFactory)
        {
            _config = config;
            _responseFactory = responseFactory;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if API key header exists
            var requestApiKey = context.HttpContext.Request.Headers["x-api-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(requestApiKey))
            {
                context.Result = new ObjectResult(_responseFactory.Failure(Statuses.ApiKeyMissing))
                {
                    StatusCode = Statuses.ApiKeyMissing.HttpCode
                };
                return;
            }

            // Read valid API key from config
            var validApiKey = _config["Security:ApiKey"];

            if (string.IsNullOrWhiteSpace(validApiKey))
            {
                context.Result = new ObjectResult(_responseFactory.Failure(Statuses.ApiKeyNotConfigured))
                {
                    StatusCode = Statuses.ApiKeyNotConfigured.HttpCode
                };
                return;
            }

            // Timing-safe comparison
            var isValid = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(requestApiKey), 
                Encoding.UTF8.GetBytes(validApiKey));

            if (!isValid)
            {
                context.Result = new ObjectResult(_responseFactory.Failure(Statuses.ApiKeyInvalid))
                {
                    StatusCode = Statuses.ApiKeyInvalid.HttpCode
                };
            }
        }
    }
}
