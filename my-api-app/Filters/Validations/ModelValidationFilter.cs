using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace my_api_app.Filters.Validations
{
    public class ModelValidationFilter : IActionFilter
    {
        // Runs BEFORE the action method
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // Short-circuit — action never executes, returns 400 immediately
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        // Runs AFTER the action method
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing needed here for validation
        }
    }
}
