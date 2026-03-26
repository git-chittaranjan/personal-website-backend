using Microsoft.Data.SqlClient;
using my_api_app.Infrastructure.Database;
using System.Data;
using static System.Net.WebRequestMethods;

namespace my_api_app.Repositories.HealthRepo
{
    public class HealthRepository : IHealthRepository
    {
        private readonly IDbConnectionFactory _factory;

        public HealthRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
        public async Task<bool> PingAsync()
        {
            try
            {
                const string sql = "SELECT 'warm' AS Status, GETDATE() AS PingedAt";

                using SqlConnection con = _factory.CreateConnection();
                using SqlCommand cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await cmd.ExecuteScalarAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
