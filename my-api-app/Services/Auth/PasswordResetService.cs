using my_api_app.DTOs.Auth;
using my_api_app.Enums;
using my_api_app.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Exceptions.BusinessExceptions.TokenExceptions;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;
using my_api_app.Services.Security.Implementations;
using my_api_app.Services.Security.Interfaces;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;

namespace my_api_app.Services.Auth
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordResetTokenRepository _tokenRepo;
        private readonly IPasswordHasher _hasher;
        private readonly IResetTokenHasher _tokenHasher;
        private readonly IConfiguration _config;
        private readonly IOtpService _otpService;

        public PasswordResetService(IUserRepository userRepo, IPasswordResetTokenRepository tokenRepo, IPasswordHasher hasher, IResetTokenHasher tokenHasher, IConfiguration config, IOtpService otpService)
        {
            _userRepo = userRepo;
            _tokenRepo = tokenRepo;
            _hasher = hasher;
            _tokenHasher = tokenHasher;
            _config = config;
            _otpService = otpService;
        }



        // ------------------------------
        // Forgot Password : Validate Email and Send OTP
        // ------------------------------
        public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetUserAsync(email, cancellationToken);

            if (user == null)
                return;

            await _otpService.GenerateAndSendOtpAsync(user.Name, user.Email, OtpPurpose.PASSWORD_RESET, cancellationToken);
        }


        public async Task<ForgotPasswordResponseDto> GenerateResetTokenAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userRepo.GetUserAsync(email, cancellationToken);
            if (user == null)
                throw new InternalServerException();

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            int expiryMinutes = int.Parse(_config["ResetToken:ExpiryMinutes"] ?? "10");

            var hashToken = _tokenHasher.Hash(rawToken);

            var tokenEntity = new PasswordResetToken
            {
                Email = user.Email,
                TokenHash = hashToken, //Saving hash token in DB
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };

            await _tokenRepo.CreateResetTokenAsync(tokenEntity, cancellationToken);

            var forgotPasswordResponse = new ForgotPasswordResponseDto
            {
                ResetToken = rawToken, //Sending raw token in the response
                ExpiresAt = tokenEntity.ExpiresAt,
                TokenType = "Bearer"
            };

            return forgotPasswordResponse;
        }



        public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenHasher.Hash(token);

            var passwordResetToken = await _tokenRepo.GetResetTokenAsync(tokenHash, cancellationToken);

            // Token not found, already used, or expired
            if (passwordResetToken == null)
                throw new InvalidPasswordResetTokenException();

            // Token does not belong to this email — possible tampering
            if (passwordResetToken.Email != email)
                throw new InvalidPasswordResetTokenException();

            var (hash, salt) = _hasher.HashPassword(newPassword);

            await _userRepo.UpdatePasswordAsync(passwordResetToken.Email, hash, salt, cancellationToken);
            await _tokenRepo.MarkAsUsedResetTokenAsync(passwordResetToken.TokenID, cancellationToken);
        }
    }
}
