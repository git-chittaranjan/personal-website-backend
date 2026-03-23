using Microsoft.Data.SqlClient;
using my_api_app.Exceptions.BusinessExceptions;

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
            //var connectionString = _configuration.GetConnectionString("AzureSqlServerConnection");

            var connectionString = _configuration.GetConnectionString("LocalSqlServerConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationException(
                                configKey: "ConnectionStrings:LocalSqlServerConnection",
                                detail: "Connection string is missing from appsettings."
                            ); //Middleware will catch and log this.

            return new SqlConnection(connectionString);
        }
    }
}
