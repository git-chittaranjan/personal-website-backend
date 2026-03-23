using Microsoft.AspNetCore.Mvc.Filters;
using Serilog.Context;
using System.Security.Claims;

namespace my_api_app.Core.Filters.Logging
{
    public class ActionLoggingFilter : IAsyncActionFilter
    {
        private readonly ILogger<ActionLoggingFilter> _logger;

        // Action arguments containing these keys will be redacted
        private static readonly HashSet<string> RedactedArguments = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "confirm_password", "reset_token", "otp_code", "new_password"
        };

        public ActionLoggingFilter(ILogger<ActionLoggingFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
        {
            var controllerName = executingContext.RouteData.Values["controller"]?.ToString();
            var actionName = executingContext.RouteData.Values["action"]?.ToString();
            var userId = executingContext.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            // Redact sensitive argument values before logging
            var safeArguments = executingContext.ActionArguments
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => RedactedArguments.Contains(kvp.Key) ? "[Redacted]" : kvp.Value
                );

            using (LogContext.PushProperty("Controller", controllerName))
            using (LogContext.PushProperty("Action", actionName))
            {
                // --- Log before action exection starts ---
                using (LogContext.PushProperty("EventType", "ActionExecuting"))
                {
                    _logger.LogInformation(
                        "Executing {Controller}.{Action} | Args:{Arguments} | User:{UserId}",
                        controllerName,
                        actionName,
                        safeArguments,
                        userId);
                }

                // --- Execute the action ---
                var executedContext = await next();

                // --- Log after action exection ends ---
                using (LogContext.PushProperty("EventType", "ActionExecuted"))
                {
                    if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
                    {
                        // DO NOT log exception here. GlobalExceptionMiddleware owns exception logging with full context
                        // Just log that the action exited with an exception — no stack trace
                        _logger.LogWarning(
                            "Action {Controller}.{Action} exited with exception — handled by GlobalExceptionMiddleware | User:{UserId}",
                            controllerName, actionName, userId);
                    }
                    else
                    {
                        var resultType = executedContext.Result?.GetType().Name ?? "null";
                        _logger.LogInformation(
                            "Executed {Controller}.{Action} | Result:{ResultType} | User:{UserId}",
                            controllerName, actionName, resultType, userId);
                    }
                }
            }
        }
    }
}
