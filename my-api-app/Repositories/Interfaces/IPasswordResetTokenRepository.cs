using my_api_app.Models.Auth;

namespace my_api_app.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<bool> CreateResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken);
        Task<PasswordResetToken?> GetResetTokenAsync(byte[] tokenHash, CancellationToken cancellationToken);
        Task<bool> MarkAsUsedResetTokenAsync(Guid tokenId, CancellationToken cancellationToken);
    }
}
