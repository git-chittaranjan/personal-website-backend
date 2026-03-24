using my_api_app.Core.Exceptions.BusinessExceptions.UserExceptions;
using my_api_app.Core.Responses;
using my_api_app.Domain.Models;
using my_api_app.Features.User.DTOs;
using my_api_app.Features.User.DTOs.CreateUser;
using my_api_app.Features.User.DTOs.GetUsers;
using my_api_app.Features.User.DTOs.PatchUser;
using my_api_app.Features.User.DTOs.UpdateUser;
using my_api_app.Infrastructure.Security.Hasher;
using my_api_app.Repositories.UserRepo;

namespace my_api_app.Features.User.Services
{

    /// <summary>
    /// Concrete implementation of IUserService.
    /// Orchestrates domain logic, repository access, and DTO mapping.
    /// No HTTP concerns exist here — just pure business rules.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IPasswordHasher hasher, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = hasher;
            _logger = logger;
        }



        // ------------------------------
        // CREATE User in the Users table without OTP
        // ------------------------------
        //CancellationToken cancellationToken = default provides the default value for the CancellationToken struct, which is equivalent to CancellationToken.None, that signals no cancellation is requested.
        public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("CreateUserAsync - Checking if email already exists: {Email}", request.Email);

            bool emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);

            if (emailExists)
            {
                _logger.LogWarning("CreateUserAsync - User creation failed — email already exists: {Email}", request.Email);

                throw new UserAlreadyExistsException();
            }

            var (hash, salt) = _passwordHasher.HashPassword(request.Password);

            var user = new Domain.Models.User
            {
                Name = request.Name,
                Gender = request.Gender,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            CreatedUserResult result = await _userRepository.CreateUserAsync(user.Name, user.Email, user.Gender, user.PasswordHash, user.PasswordSalt, cancellationToken);

            _logger.LogInformation("CreateUserAsync - User created successfully — UserId: {UserId}, Email: {Email}", result.UserID, request.Email);

            return new CreateUserResponseDto
            {
                UserId = result.UserID,
                Email = request.Email,
                CreatedAt = result.CreatedAt
            };
        }



        // ------------------------------
        // GET USER BY ID
        // ------------------------------
        public async Task<GetUserResponseDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetUserByIdAsync - Fetching user by UserId: {UserId}", userId);

            UserDetails user = await _userRepository.GetUserByIdAsync(userId, cancellationToken)
                       ?? throw new UserNotFoundException();

            return new GetUserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Gender = user.Gender,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }



        // ------------------------------
        // GET ALL USERS
        // ------------------------------
        public async Task<(List<GetUserResponseDto> Users, Pagination Pagination)> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            // Guards in service as well — service should be callable outside controller too
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            _logger.LogInformation("GetAllUsersAsync - Fetching all users — Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var (items, totalCount) = await _userRepository.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);

            _logger.LogInformation("GetAllUsersAsync - Fetched {Count} users out of {TotalCount}", items.Count(), totalCount);

            var users = items.Select(MapToResponse).ToList();

            var pagination = new Pagination
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return (users, pagination);
        }



        // ------------------------------
        // UPDATE
        // ------------------------------
        public async Task<GetUserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("UpdateUserAsync - Fetching user for update — UserId: {UserId}", id);

            var userDetails = await _userRepository.GetUserByIdAsync(id, cancellationToken)
                       ?? throw new UserNotFoundException();

            userDetails.Name = request.Name;
            userDetails.Gender = request.Gender;

            _logger.LogInformation("UpdateUserAsync - Updating user — UserId: {UserId}", id);

            var updatedUser = await _userRepository.UpdateUserAsync(userDetails, cancellationToken);

            if (updatedUser == null)
                throw new UserNotFoundException();

            _logger.LogInformation("UpdateUserAsync - User updated successfully — UserId: {UserId}", id);

            return MapToResponse(updatedUser);
        }



        // ------------------------------
        // PATCH UPDATE
        // ------------------------------
        public async Task<GetUserResponseDto> PatchUserAsync(Guid id, PatchUserRequestDto request, CancellationToken cancellationToken = default)
        {
            var userDetails = await _userRepository.GetUserByIdAsync(id, cancellationToken)
                ?? throw new UserNotFoundException();

            // Map only the fields that are provided (PATCH = partial update)
            if (!string.IsNullOrWhiteSpace(request.Name))
                userDetails.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Gender.ToString()))
                userDetails.Gender = request.Gender;

            _logger.LogInformation("PatchUserAsync - Patching user — UserId: {UserId}", id);

            var patched = await _userRepository.PatchUserAsync(id, userDetails, cancellationToken);

            if (patched is null)
                throw new UserNotFoundException();

            _logger.LogInformation("PatchUserAsync - User patched successfully — UserId: {UserId}", id);

            return MapToResponse(patched);
        }



        // ------------------------------
        // DELETE
        // ------------------------------
        public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new InvalidUserIdException();

            _logger.LogInformation("DeleteUserAsync - Fetching user for deletion — UserId: {UserId}", userId);

            UserDetails user = await _userRepository.GetUserByIdAsync(userId, cancellationToken)
                     ?? throw new UserNotFoundException();

            _logger.LogWarning("DeleteUserAsync - Deleting user — UserId: {UserId}", userId);

            var deleted = await _userRepository.DeleteUserAsync(userId, cancellationToken);

            if (!deleted)
                throw new UserDeletionFailedException();

            _logger.LogWarning("DeleteUserAsync - User deleted successfully — UserId: {UserId}", userId);
        }



        // ------------------------------
        // HELPERS FUNCTION
        // ------------------------------
        private static GetUserResponseDto MapToResponse(UserDetails user) => new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Gender = user.Gender,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
