namespace my_api_app.Features.Health.DTOs
{
    public class HealthStatusDto
    {
        public string Status { get; set; } = default!;
        public string Database { get; set; } = default!;
        public DateTime PingedAt { get; set; }
    }
}
