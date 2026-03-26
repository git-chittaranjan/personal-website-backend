using my_api_app.Features.Health.DTOs;

namespace my_api_app.Features.Health.Services
{
    public interface IHealthService
    {
        Task<HealthStatusDto> GetHealthStatusAsync();
    }
}
