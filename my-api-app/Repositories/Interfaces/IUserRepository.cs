using my_api_app.Enums;
using my_api_app.Models.Auth;

namespace my_api_app.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
        Task<CreatedUserResult> CreateUserAsync(string name, string email, Gender? gender, byte[] hash, byte[] salt, CancellationToken cancellationToken);
        Task<User?> GetUserAsync(string email, CancellationToken cancellationToken);
        Task<bool> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt, CancellationToken cancellationToken);
    }
}
