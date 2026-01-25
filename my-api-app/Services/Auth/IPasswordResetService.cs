using my_api_app.DTOs.Auth;

namespace my_api_app.Services.Auth
{
    public interface IPasswordResetService
    {
        Task ForgotPasswordAsync(string email, CancellationToken cancellationToken);
        Task<ForgotPasswordResponseDto> GenerateResetTokenAsync(string email, CancellationToken cancellationToken);
        Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
    }
}
