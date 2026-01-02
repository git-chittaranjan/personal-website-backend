namespace my_api_app.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string otpCode, int expiryMinutes);
    }
}
