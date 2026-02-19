using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace my_api_app.Exceptions.Controllers
{
    [ApiController]
    [Route("api")]
    public sealed class ErrorController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ErrorController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("error")]
        public IActionResult HandleError()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;

            var problemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _webHostEnvironment.IsDevelopment() ? exception?.Message : "An unexpected error occurred while processing your request. Please try again later."
            };

            problemDetails.Extensions["trace_id"] = HttpContext.TraceIdentifier;

            return StatusCode(problemDetails.Status.Value, problemDetails);
        }
    }
}