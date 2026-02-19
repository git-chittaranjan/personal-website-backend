namespace my_api_app.Middlewares.Logging
{
    public class ConsoleRequestLogger
    {
        private readonly RequestDelegate _next;

        public ConsoleRequestLogger(RequestDelegate next)
        {
            _next = next;
        }

        // This runs for EVERY request
        public async Task InvokeAsync(HttpContext context)
        {
            // BEFORE next middleware: Log incoming request ([10:24:15] GET /api/auth/login)
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {context.Request.Method} {context.Request.Path}");

            // Pass to the NEXT MIDDLEWARE in the pipelin
            await _next(context);

            // AFTER response: Log completion ([10:24:17] Response sent: 200)
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Response sent: {context.Response.StatusCode}");
        }
    }
}
