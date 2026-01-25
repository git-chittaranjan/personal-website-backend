using Microsoft.Data.SqlClient;

namespace my_api_app.Data
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
