using my_api_app.Core.Responses;
using my_api_app.Features.User.DTOs;
using my_api_app.Features.User.DTOs.CreateUser;
using my_api_app.Features.User.DTOs.GetUsers;
using my_api_app.Features.User.DTOs.PatchUser;
using my_api_app.Features.User.DTOs.UpdateUser;

namespace my_api_app.Features.User.Services
{
    public interface IUserService
    {
        Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);

        Task<GetUserResponseDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(List<GetUserResponseDto> Users, Pagination Pagination)> GetAllUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<GetUserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);

        Task<GetUserResponseDto> PatchUserAsync(Guid id, PatchUserRequestDto request, CancellationToken cancellationToken = default);

        Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
