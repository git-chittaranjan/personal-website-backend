using Microsoft.Data.SqlClient;

namespace my_api_app.Data
{
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public SqlServerConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            //return new SqlConnection(_configuration.GetConnectionString("AzureSqlServerConnection"));

            return new SqlConnection(_configuration.GetConnectionString("LocalSqlServerConnection"));            
        }
    }
}
