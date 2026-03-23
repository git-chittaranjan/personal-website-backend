using Microsoft.AspNetCore.HttpLogging;

namespace my_api_app.Core.Extensions
{
    public static class AddHttpLoggingExtensions //For UseHttpLogging() built-in Middleware
    {
        public static IServiceCollection AddHttpLoggingConfiguration(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields =
                    HttpLoggingFields.RequestMethod
                    | HttpLoggingFields.RequestPath
                    | HttpLoggingFields.RequestQuery
                    | HttpLoggingFields.RequestProtocol
                    | HttpLoggingFields.RequestScheme
                    | HttpLoggingFields.RequestHeaders
                    | HttpLoggingFields.ResponseStatusCode
                    | HttpLoggingFields.ResponseHeaders
                    | HttpLoggingFields.Duration;

                // Request headers
                logging.RequestHeaders.Add("X-Request-Id");
                logging.RequestHeaders.Add("X-Forwarded-For");
                logging.RequestHeaders.Add("Content-Type");
                logging.RequestHeaders.Add("Accept");
                logging.RequestHeaders.Add("User-Agent");

                // Response headers
                logging.ResponseHeaders.Add("X-Correlation-Id");
                logging.ResponseHeaders.Add("Content-Type");
                logging.ResponseHeaders.Add("Cache-Control");

                // Media types allowed for body logging
                logging.MediaTypeOptions.AddText("application/json");
                logging.MediaTypeOptions.AddText("application/xml");

                logging.CombineLogs = true;

                if (environment.IsProduction())
                {
                    logging.RequestBodyLogLimit = 0;
                    logging.ResponseBodyLogLimit = 0;
                }
                else
                {
                    logging.LoggingFields |= HttpLoggingFields.RequestBody
                                           | HttpLoggingFields.ResponseBody;
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;
                }
            });

            return services;
        }
    }
}
