using Microsoft.Data.SqlClient;

namespace my_api_app.Infrastructure.Database
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
