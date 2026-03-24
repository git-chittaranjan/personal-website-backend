using my_api_app.Core.Exceptions.BusinessExceptions.OtpExceptions;
using my_api_app.Core.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Core.Helpers;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.Infrastructure.Email;
using my_api_app.Repositories.Auth;

namespace my_api_app.Infrastructure.OTP
{
    public sealed class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<OtpService> _logger;


        public OtpService(IOtpRepository otpRepository, IEmailService emailService, IConfiguration config, ILogger<OtpService> logger)
        {
            _otpRepository = otpRepository;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }



        public async Task GenerateAndSendOtpAsync(string name, string email, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GenerateAndSendOtpAsync - Generating OTP for {Email} — Purpose: {Purpose}", email, purpose);

            string otp = OtpGenerator.GenerateSixDigitOtp();

            int expiryMinutes = int.TryParse(_config["Otp:ExpiryMinutes"], out var value) ? value : 5;
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var flag = await _otpRepository.SaveOtpAsync(email, otp, expiresAt, purpose, cancellationToken);

            if (!flag)
            {
                _logger.LogInformation("GenerateAndSendOtpAsync - Failed saving OTP to DB for {Email} — Purpose: {Purpose}", email, purpose);

                throw new InternalServerException();
            }

            _logger.LogInformation("GenerateAndSendOtpAsync - OTP saved to DB for {Email} — Purpose: {Purpose}, ExpiresAt: {ExpiresAt}", email, purpose, expiresAt);

            try
            {
                await _emailService.SendEmailAsync(name, email, otp, expiryMinutes, purpose);

                _logger.LogInformation("GenerateAndSendOtpAsync - OTP email dispatched successfully to {Email} — Purpose: {Purpose}", email, purpose);
            }
            catch (Exception)
            {
                _logger.LogInformation("GenerateAndSendOtpAsync - OTP delivery failed {Email} — Purpose: {Purpose}", email, purpose);

                throw new OtpDeliveryFailedException();
            }
        }



        public async Task<Guid> ValidateOtpAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ValidateOtpAsync - Validating OTP for {Email} — Purpose: {Purpose}", email, purpose);

            // Validate OTP purpose
            if (!Enum.IsDefined(typeof(OtpPurpose), purpose))
                throw new UnsupportedOtpPurposeException();

            OtpEntry? otpEntry = await _otpRepository.GetLatestOtpAsync(email, purpose, cancellationToken);

            // No otp record found
            if (otpEntry == null)
            {
                _logger.LogWarning("ValidateOtpAsync - OTP validation failed — no OTP record found for {Email}, Purpose: {Purpose}", email, purpose);

                throw new InvalidOtpException();
            }

            // User entered OTP code does not match
            if (otpEntry.OtpCode != otp)
            {
                _logger.LogWarning("ValidateOtpAsync - OTP validation failed — code mismatch for {Email}, Purpose: {Purpose}", email, purpose);

                throw new InvalidOtpException();
            }

            // Otp already used
            if (otpEntry.IsUsed)
            {
                _logger.LogWarning("ValidateOtpAsync - OTP validation failed — OTP already used for {Email}, Purpose: {Purpose}", email, purpose);

                throw new OtpAlreadyUsedException();
            }

            // Otp expired
            if (otpEntry.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("ValidateOtpAsync - OTP validation failed — OTP expired for {Email}, Purpose: {Purpose}, ExpiredAt: {ExpiresAt}", email, purpose, otpEntry.ExpiresAt);

                throw new OtpExpiredException();
            }

            // All checks passed — mark as used
            var flag = await _otpRepository.MarkOtpAsUsedAsync(otpEntry.OtpID, cancellationToken);

            if (!flag)
            {
                _logger.LogWarning("ValidateOtpAsync - Failed to mark OTP as used for {Email}, Purpose: {Purpose}", email, purpose);

                throw new InternalServerException();
            }

            _logger.LogInformation("ValidateOtpAsync - OTP validated successfully for {Email} — Purpose: {Purpose}, OtpId: {OtpId}", email, purpose, otpEntry.OtpID);

            return otpEntry.OtpID;
        }
    }
}
