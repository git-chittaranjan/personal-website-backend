using my_api_app.Domain.Models;

namespace my_api_app.Repositories.User
{
    public interface IPendingUserRepository
    {
        Task<bool> CreatePendingUserAsync(Domain.Models.User pendingUser, CancellationToken cancellationToken);
        Task<Domain.Models.User?> GetPendingUserAsync(string email, CancellationToken cancellationToken);
        Task<bool> DeletePendingUserAsync(string email, CancellationToken cancellationToken);
    }
}
