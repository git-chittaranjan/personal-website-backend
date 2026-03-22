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
        private readonly ILogger<ApiKeyFilter> _logger;

        public ApiKeyFilter(IConfiguration config, IApiResponseFactory responseFactory, ILogger<ApiKeyFilter> logger)
        {
            _config = config;
            _responseFactory = responseFactory;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if API key header exists
            var requestApiKey = context.HttpContext.Request.Headers["x-api-key"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(requestApiKey))
            {
                _logger.LogWarning("x-api-key header missing | Path:{Path}", context.HttpContext.Request.Path);

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
                _logger.LogCritical("API key not configured in Security:ApiKey");

                context.Result = new ObjectResult(_responseFactory.Failure(Statuses.ApiKeyNotConfigured))
                {
                    StatusCode = Statuses.ApiKeyNotConfigured.HttpCode
                };
                return;
            }

            // Timing-safe comparison
            var requestBytes = Encoding.UTF8.GetBytes(requestApiKey);
            var validBytes = Encoding.UTF8.GetBytes(validApiKey);

            var isValid = requestBytes.Length == validBytes.Length
                && CryptographicOperations.FixedTimeEquals(requestBytes, validBytes);

            if (!isValid)
            {
                _logger.LogWarning("Invalid API key attempt | Path:{Path} | IP:{Ip}", context.HttpContext.Request.Path, context.HttpContext.Connection.RemoteIpAddress);

                context.Result = new ObjectResult(_responseFactory.Failure(Statuses.ApiKeyInvalid))
                {
                    StatusCode = Statuses.ApiKeyInvalid.HttpCode
                };
            }
        }
    }
}
