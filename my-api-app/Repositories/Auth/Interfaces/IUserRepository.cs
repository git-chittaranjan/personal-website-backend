using my_api_app.DTOs.User;
using my_api_app.Enums;
using my_api_app.Models.Auth;
using my_api_app.Models.User;

namespace my_api_app.Repositories.Auth.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
        Task<CreatedUserResult> CreateUserAsync(string name, string email, Gender? gender, byte[] hash, byte[] salt, CancellationToken cancellationToken);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
        Task<UserDetails?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt, CancellationToken cancellationToken);
        Task<(IEnumerable<UserDetails> Items, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<UserDetails?> UpdateUserAsync(UserDetails user, CancellationToken cancellationToken);
        Task<UserDetails?> PatchUserAsync(Guid id, PatchUserRequestDto request, CancellationToken cancellationToken);
        Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    }
}
