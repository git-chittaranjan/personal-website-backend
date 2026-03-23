
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_api_app.Core.Filters.Logging;
using my_api_app.Core.Responses;
using my_api_app.Features.User.DTOs;
using my_api_app.Features.User.DTOs.CreateUser;
using my_api_app.Features.User.DTOs.GetUsers;
using my_api_app.Features.User.DTOs.PatchUser;
using my_api_app.Features.User.DTOs.UpdateUser;
using my_api_app.Features.User.Services;
using System.Security.Claims;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[ServiceFilter(typeof(ActionLoggingFilter))] -- Applied Globally in Program.cs
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IApiResponseFactory apiResponseFactory, ILogger<UsersController> logger) : base(apiResponseFactory)
        {
            _userService = userService;
            _logger = logger;
        }

        private string GetAdminId() => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";



        // ------------------------------
        // CREATE USER
        // ------------------------------
        [HttpPost()]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
        {
            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} creating user with Email: {Email}", adminId, request.Email);

            CreateUserResponseDto user = await _userService.CreateUserAsync(request, cancellationToken);

            _logger.LogInformation("Admin {AdminId} created user successfully with UserId: {UserId}", adminId, user.UserId);

            return CreatedResponse(Statuses.UserCreated, user);
        }



        // ------------------------------
        // GET USER BY ID
        // ------------------------------
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} fetching user with UserId: {UserId}", adminId, id);

            GetUserResponseDto user = await _userService.GetUserByIdAsync(id, cancellationToken);

            return SuccessResponse(Statuses.Success, user);
        }



        // ------------------------------
        // GET ALL USERS
        // ------------------------------
        [HttpGet()]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} fetching all users - Page {PageNumber}, Size {PageSize}", adminId, pageNumber, pageSize);

            if (pageNumber < 1) pageNumber = 1; //pageNumber: Current page number being requested
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; //PageSize: Number of records to be returned per page

            var (users, pagination) = await _userService.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);

            return SuccessResponse(Statuses.Success, users, pagination);
        }



        // ------------------------------
        // PUT: UPDATE USER
        // ------------------------------
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request, CancellationToken cancellationToken)
        {
            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} updating user with UserId: {UserId}", adminId, id);

            GetUserResponseDto user = await _userService.UpdateUserAsync(id, request, cancellationToken);

            _logger.LogInformation("Admin {AdminId} updated user successfully with UserId: {UserId}", adminId, user.UserId);

            return SuccessResponse(Statuses.UserUpdated, user);
        }



        // ------------------------------
        // PATCH: UPDATE USER
        // ------------------------------
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PatchUser(Guid id, [FromBody] PatchUserRequestDto request, CancellationToken cancellationToken)
        {
            var adminId = GetAdminId();
            _logger.LogInformation("Admin {AdminId} patching user with UserId: {UserId}", adminId, id);

            GetUserResponseDto user = await _userService.PatchUserAsync(id, request, cancellationToken);

            _logger.LogInformation("Admin {AdminId} patched user successfully with UserId: {UserId}", adminId, user.UserId);

            return SuccessResponse(Statuses.UserUpdated, user);
        }



        // ------------------------------
        // DELETE USER
        // ------------------------------
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            var adminId = GetAdminId();
            _logger.LogWarning("Admin {AdminId} deleting user with UserId: {UserId}", adminId, id);

            await _userService.DeleteUserAsync(id, cancellationToken);

            _logger.LogWarning("Admin {AdminId} deleted user successfully with UserId: {UserId}", adminId, id);

            return SuccessResponse(Statuses.UserDeleted);
        }
    }
}
