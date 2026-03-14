using my_api_app.DTOs.User;
using my_api_app.Enums;
using my_api_app.Exceptions.BusinessExceptions;
using my_api_app.Exceptions.BusinessExceptions.UserExceptions;
using my_api_app.Models.Auth;
using my_api_app.Models.User;
using my_api_app.Repositories.Auth.Interfaces;
using my_api_app.Services.Security.Interfaces;

namespace my_api_app.Services.User
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

        public UserService(IUserRepository userRepository, IPasswordHasher _hasher)
        {
            _userRepository = userRepository;
            _passwordHasher = _hasher;
        }



        // ------------------------------
        // CREATE User in the Users table without OTP
        // ------------------------------
        //CancellationToken cancellationToken = default provides the default value for the CancellationToken struct, which is equivalent to CancellationToken.None, that signals no cancellation is requested.
        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
        {
            bool emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);

            if (emailExists)
                throw new UserAlreadyExistsException();

            var (hash, salt) = _passwordHasher.HashPassword(request.Password);

            var user = new Models.Auth.User
            {
                Name = request.Name,
                Gender = request.Gender,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsEmailVerified = true
            };

            CreatedUserResult result = await _userRepository.CreateUserAsync(user.Name, user.Email, user.Gender, user.PasswordHash, user.PasswordSalt, cancellationToken);

            return new CreateUserResponse
            {
                UserId = result.UserID,
                Email = request.Email,
                CreatedAt = result.CreatedAt
            };
        }



        // ------------------------------
        // GET USER BY ID
        // ------------------------------
        public async Task<UserResponseDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            UserDetails user = await _userRepository.GetUserByIdAsync(userId, cancellationToken)
                       ?? throw new UserNotFoundException();

            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Gender = user.Gender,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
            };
        }



        // ------------------------------
        // GET ALL USERS
        // ------------------------------
        public async Task<PagedResult<UserResponseDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // hard cap — prevent runaway queries

            var (items, totalCount) = await _userRepository.GetAllUsersAsync(pageNumber, pageSize, cancellationToken);

            return new PagedResult<UserResponseDto>
            {
                Items = items.Select(MapToResponse),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }



        // ------------------------------
        // UPDATE
        // ------------------------------
        public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByIdAsync(id, cancellationToken)
                       ?? throw new UserNotFoundException();

            user.Name = request.Name;
            user.Gender = request.Gender;

            var updated = await _userRepository.UpdateUserAsync(user, cancellationToken);

            if (updated == null)
                throw new UserNotFoundException();

            return MapToResponse(updated);
        }



        // ------------------------------
        // PATCH UPDATE
        // ------------------------------
        public async Task<UserResponseDto> PatchUserAsync(Guid id, PatchUserRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByIdAsync(id, cancellationToken)
                       ?? throw new UserNotFoundException();

            var patched = await _userRepository.PatchUserAsync(id, request, cancellationToken);

            if (patched is null)
                throw new UserNotFoundException();

            return MapToResponse(patched);
        }



        // ------------------------------
        // DELETE
        // ------------------------------
        public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new InvalidUserIdException();

            UserDetails user = await _userRepository.GetUserByIdAsync(userId, cancellationToken)
                     ?? throw new UserNotFoundException();

            var deleted = await _userRepository.DeleteUserAsync(userId, cancellationToken);

            if (!deleted)
                throw new UserDeletionFailedException();
        }



        // ------------------------------
        // HELPERS FUNCTION
        // ------------------------------
        private static UserResponseDto MapToResponse(UserDetails user) => new()
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
