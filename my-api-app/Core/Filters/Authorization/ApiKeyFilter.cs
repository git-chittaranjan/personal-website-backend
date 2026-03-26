using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Responses;
using System.Security.Cryptography;
using System.Text;

namespace my_api_app.Core.Filters.Authorization
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
            // Check for skip attribute
            var hasSkipAttribute = context.ActionDescriptor.EndpointMetadata
                .Any(em => em is SkipApiKeyAuthAttribute);

            if (hasSkipAttribute)
            {
                return; // Bypass API key validation completely
            }

            // Check if API key header exists
            var requestApiKey = context.HttpContext.Request.Headers["x-api-key"].FirstOrDefault();
            var userId = context.HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            if (string.IsNullOrWhiteSpace(requestApiKey))
            {
                _logger.LogWarning("x-api-key request header missing | Path:{Path} | User:{UserId}", context.HttpContext.Request.Path, userId);

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
                throw new ConfigurationException(
                       configKey: "Security:ApiKey",
                       detail: "is missing from appsettings."
                ); //Middleware will catch and log this.
            }

            // Timing-safe comparison
            var requestBytes = Encoding.UTF8.GetBytes(requestApiKey);
            var validBytes = Encoding.UTF8.GetBytes(validApiKey);

            var isValid = requestBytes.Length == validBytes.Length
                && CryptographicOperations.FixedTimeEquals(requestBytes, validBytes);

            if (!isValid)
            {
                _logger.LogWarning("Invalid API key attempt | Path:{Path} | IP:{Ip} | User:{UserId}", context.HttpContext.Request.Path, context.HttpContext.Connection.RemoteIpAddress, userId);

                context.Result = new ObjectResult(_responseFactory.Failure(Statuses.ApiKeyInvalid))
                {
                    StatusCode = Statuses.ApiKeyInvalid.HttpCode
                };
            }
        }
    }
}
