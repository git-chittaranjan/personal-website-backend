using my_api_app.Domain.Enums;

namespace my_api_app.Infrastructure.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string naame, string to, string otpCode, int expiryMinutes, OtpPurpose purpose);
    }
}
