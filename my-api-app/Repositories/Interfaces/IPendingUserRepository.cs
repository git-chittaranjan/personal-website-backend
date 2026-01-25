using my_api_app.Models.Auth;

namespace my_api_app.Repositories.Interfaces
{
    public interface IPendingUserRepository
    {
        Task<bool> CreatePendingUserAsync(User pendingUser, CancellationToken cancellationToken);
        Task<User?> GetPendingUserAsync(string email, CancellationToken cancellationToken);
        Task<bool> DeletePendingUserAsync(string email, CancellationToken cancellationToken);
    }
}
