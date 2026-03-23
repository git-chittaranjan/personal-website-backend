using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;

namespace my_api_app.Repositories.Auth
{
    public interface IOtpRepository
    {
        Task<bool> SaveOtpAsync(string email, string otp, DateTime expiresAt, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<OtpEntry?> GetLatestOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<bool> MarkOtpAsUsedAsync(Guid otpId, CancellationToken cancellationToken);
    }
}
