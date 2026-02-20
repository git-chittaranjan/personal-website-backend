namespace my_api_app.Middlewares.Logging
{
    public static class ConsoleRequestLoggerExtension
    {
        public static IApplicationBuilder UseConsoleRequestLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConsoleRequestLogger>();
        }
    }
}
