namespace my_api_app.Middlewares.Logging
{
    public static class ConsoleRequestLoggerExtensions
    {
        public static IApplicationBuilder UseConsoleRequestLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConsoleRequestLogger>();
        }
    }
}
