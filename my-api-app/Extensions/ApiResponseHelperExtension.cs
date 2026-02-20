using my_api_app.Helpers;

namespace my_api_app.Extensions
{
    public static class ApiResponseHelperExtension
    {
        public static WebApplication ConfigureApiResponseHelper(this WebApplication app)
        {
            ApiResponseFactory.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
}
