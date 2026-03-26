namespace my_api_app.Repositories.HealthRepo
{
    public interface IHealthRepository
    {
        Task<bool> PingAsync();
    }
}
