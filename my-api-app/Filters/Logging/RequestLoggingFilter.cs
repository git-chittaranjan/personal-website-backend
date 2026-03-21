using Microsoft.AspNetCore.Mvc.Filters;

namespace my_api_app.Filters.Logging
{
    public class RequestLoggingFilter : IAsyncActionFilter
    {
        private readonly ILogger<RequestLoggingFilter> _logger;

        public RequestLoggingFilter(ILogger<RequestLoggingFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Before — record start time and log request
            var actionName = context.ActionDescriptor.DisplayName;
            var path = context.HttpContext.Request.Path;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation("Starting action: {ActionName} | Path: {Path}", actionName, path);

            var executedContext = await next(); // execute action

            // After — log execution time and result
            stopwatch.Stop();

            if (executedContext.Exception != null)
            {
                _logger.LogError("Action: {ActionName} failed in {ElapsedMs}ms",
                    actionName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("Completed action: {ActionName} in {ElapsedMs}ms", 
                    actionName, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
