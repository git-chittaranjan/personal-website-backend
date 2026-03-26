using Microsoft.Data.SqlClient;
using my_api_app.Features.Health.DTOs;
using my_api_app.Infrastructure.Database;
using my_api_app.Repositories.HealthRepo;
using System.Data;
using static System.Net.WebRequestMethods;

namespace my_api_app.Features.Health.Services
{
    public class HealthService : IHealthService
    {
        private readonly IHealthRepository _healthRepo;

        public HealthService(IHealthRepository healthRepo)
        {
            _healthRepo = healthRepo;
        }

        public async Task<HealthStatusDto> GetHealthStatusAsync()
        {
            var dbAlive = await _healthRepo.PingAsync();

            return new HealthStatusDto
            {
                Status = dbAlive ? "healthy" : "degraded",
                Database = dbAlive ? "connected" : "unreachable",
                PingedAt = DateTime.UtcNow
            };
        }
    }
}
