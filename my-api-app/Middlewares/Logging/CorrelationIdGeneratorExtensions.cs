namespace my_api_app.Middlewares.Logging
{
    public static class CorrelationIdGeneratorExtensions
    {
        public static IApplicationBuilder UseCorrelationGeneratorIdMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdGenerator>();
        }
    }
}
