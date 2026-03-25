using Microsoft.Data.SqlClient;
using my_api_app.Core.Exceptions.BusinessExceptions;

namespace my_api_app.Infrastructure.Database
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

            var connectionString = _configuration.GetConnectionString("SqlServerConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationException(
                                configKey: "ConnectionStrings:SqlServerConnection",
                                detail: "Connection string is missing from appsettings."
                            ); //Middleware will catch and log this.

            return new SqlConnection(connectionString);
        }
    }
}
