using Microsoft.AspNetCore.Mvc;
using my_api_app.Responses;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private readonly IApiResponseFactory _factory;

        protected BaseApiController(IApiResponseFactory factory)
        {
            _factory = factory;
        }

        protected IActionResult SuccessResponse(ApiStatus status)
        {
            var response = _factory.Success(status);
            return Ok(response);
        }

        protected IActionResult SuccessResponse<T>(ApiStatus status, T data, Pagination? pagination = null)
        {
            var response = _factory.Success(status, data, pagination);
            return Ok(response);
        }

        protected IActionResult CreatedResponse(ApiStatus status, string? resourceUrl = null)
        {
            var response = _factory.Success(status);

            if (!string.IsNullOrWhiteSpace(resourceUrl))
                return Created(resourceUrl, response);

            return StatusCode(201, response);
        }

        protected IActionResult CreatedResponse<T>(ApiStatus status, T data, string? resourceUrl = null)
        {
            var response = _factory.Success(status, data);

            if (!string.IsNullOrWhiteSpace(resourceUrl))
                return Created(resourceUrl, response);

            return StatusCode(201, response);
        }

        // Error response: Use this to returnn failure without throwing an exception
        protected IActionResult FailureResponse(ApiStatus status, object? errors = null)
        {
            var response = _factory.Failure(status, errors);
            return StatusCode(status.HttpCode, response);
        }
    }
}
