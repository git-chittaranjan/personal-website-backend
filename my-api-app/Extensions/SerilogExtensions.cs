using Serilog;

namespace my_api_app.Extensions
{
    public static class SerilogExtensions
    {
        public static IHostBuilder AddSerilogLogging(this IHostBuilder host)
        {
            host.UseSerilog((ctx, services, config) =>
            {
                config
                    .ReadFrom.Configuration(ctx.Configuration)   // Reads Serilog config block from appsettings.json
                    .ReadFrom.Services(services);                // Adds DI like sinks (Azure AppInsights, MS SQL Server etc.)
            });

            return host;
        }
    }
}
