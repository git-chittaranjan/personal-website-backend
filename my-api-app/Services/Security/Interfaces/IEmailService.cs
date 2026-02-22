using my_api_app.Enums;

namespace my_api_app.Services.Security.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string naame, string to, string otpCode, int expiryMinutes, OtpPurpose purpose);
    }
}
