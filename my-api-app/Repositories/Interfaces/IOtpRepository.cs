using my_api_app.Enums;
using my_api_app.Models.Auth;

namespace my_api_app.Repositories.Interfaces
{
    public interface IOtpRepository
    {
        Task<bool> SaveOtpAsync(string email, string otp, DateTime expiresAt, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<OtpEntry?> GetLatestOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<bool> MarkOtpAsUsedAsync(Guid otpId, CancellationToken cancellationToken);
    }
}
