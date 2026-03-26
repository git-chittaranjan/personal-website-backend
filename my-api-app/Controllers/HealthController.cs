using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_api_app.Core.Filters.Authorization;
using my_api_app.Core.Responses;
using my_api_app.Features.Health.DTOs;
using my_api_app.Features.Health.Services;

namespace my_api_app.Controllers
{
    public class HealthController : BaseApiController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IHealthService _healthService;

        public HealthController(IHealthService healthService, ILogger<AuthController> logger, IApiResponseFactory responseFactory)
            : base(responseFactory)
        {
            _healthService = healthService;
            _logger = logger;
        }

        /// <summary>
        /// Keep-alive endpoint — called by cron-job.org or UptimeRobot every 5 minutes.
        /// Also doubles as a health check for monitoring.
        /// </summary>
        [HttpGet("keepalive")]
        [AllowAnonymous]
        [SkipApiKeyAuth] // This bypasses api-key validation
        public async Task<IActionResult> KeepAlive([FromQuery] string key)
        {
            if (key != "1234567890")
                return Unauthorized();

            HealthStatusDto data = await _healthService.GetHealthStatusAsync();

            return data.Database == "connected"
                ? SuccessResponse(Statuses.DbPingSuccess, data)
                : FailureResponse(Statuses.DbPingFailed);
        }
    }
}
