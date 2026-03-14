
using Microsoft.AspNetCore.Mvc;
using my_api_app.DTOs.User;
using my_api_app.Responses;
using my_api_app.Services.User;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService, IApiResponseFactory apiResponseFactory) : base(apiResponseFactory)
        {
            _userService = userService;
        }



        // ------------------------------
        // CREATE USER
        // ------------------------------
        [HttpPost()]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
        {
            CreateUserResponse user = await _userService.CreateUserAsync(request, cancellationToken);

            return CreatedResponse(Statuses.UserCreated, user);
        }



        // ------------------------------
        // GET USER BY ID
        // ------------------------------
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            UserResponseDto user = await _userService.GetUserByIdAsync(id, cancellationToken);

            return SuccessResponse(Statuses.Success, user);
        }



        // ------------------------------
        // GET ALL USERS
        // ------------------------------
        [HttpGet()]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            PagedResult<UserResponseDto> result = await _userService.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);

            return SuccessResponse(Statuses.Success, result);
        }



        // ------------------------------
        // PUT: UPDATE USER
        // ------------------------------
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request, CancellationToken cancellationToken)
        {
            UserResponseDto user = await _userService.UpdateUserAsync(id, request, cancellationToken);

            return SuccessResponse(Statuses.UserUpdated, user);
        }



        // ------------------------------
        // PATCH: UPDATE USER
        // ------------------------------
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PatchUser(Guid id, [FromBody] PatchUserRequestDto request, CancellationToken cancellationToken)
        {
            UserResponseDto user = await _userService.PatchUserAsync(id, request, cancellationToken);

            return SuccessResponse(Statuses.UserUpdated, user);
        }



        // ------------------------------
        // DELETE USER
        // ------------------------------
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            await _userService.DeleteUserAsync(id, cancellationToken);

            return SuccessResponse(Statuses.UserDeleted);
        }
    }
}
