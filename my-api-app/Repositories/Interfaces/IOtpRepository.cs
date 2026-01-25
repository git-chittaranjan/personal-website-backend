using my_api_app.Enums;

namespace my_api_app.Repositories.Interfaces
{
    public interface IOtpRepository
    {
        Task<bool> SaveOtpAsync(string email, string otp, DateTime expiresAt, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<Guid?> GetValidOtpIdAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<bool> MarkOtpAsUsedAsync(Guid otpId, CancellationToken cancellationToken);
    }
}
