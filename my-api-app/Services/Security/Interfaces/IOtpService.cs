using my_api_app.Enums;

namespace my_api_app.Services.Security.Interfaces
{
    public interface IOtpService
    {
        Task GenerateAndSendOtpAsync(string name, string email, OtpPurpose purpose, CancellationToken cancellationToken);
        Task<bool> ValidateOtpAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken);
    }
}
