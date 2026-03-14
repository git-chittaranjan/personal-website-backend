using my_api_app.DTOs.User;

namespace my_api_app.Services.User
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);

        Task<UserResponseDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PagedResult<UserResponseDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);

        Task<UserResponseDto> PatchUserAsync(Guid id, PatchUserRequestDto request, CancellationToken cancellationToken = default);

        Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
