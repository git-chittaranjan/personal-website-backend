using my_api_app.Domain.Enums;

namespace my_api_app.Infrastructure.OTP
{
    public interface IOtpService
    {
        Task GenerateAndSendOtpAsync(string name, string email, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<Guid> ValidateOtpAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken);
    }
}
