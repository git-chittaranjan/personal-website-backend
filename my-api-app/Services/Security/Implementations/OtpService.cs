using my_api_app.Enums;
using my_api_app.Exceptions.BusinessExceptions.OtpExceptions;
using my_api_app.Helpers;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;
using my_api_app.Services.Security.Interfaces;

namespace my_api_app.Services.Security.Implementations
{
    public sealed class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public OtpService(IOtpRepository otpRepository, IEmailService emailService, IConfiguration config)
        {
            _otpRepository = otpRepository;
            _emailService = emailService;
            _config = config;
        }



        public async Task GenerateAndSendOtpAsync(string name, string email, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            string otp = OtpGenerator.GenerateSixDigitOtp();
            int expiryMinutes = int.Parse(_config["Otp:ExpiryMinutes"] ?? "5");
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            await _otpRepository.SaveOtpAsync(email, otp, expiresAt, purpose, cancellationToken);

            try
            {
                await _emailService.SendEmailAsync(name, email, otp, expiryMinutes, purpose);
            }
            catch (Exception)
            {
                throw new OtpDeliveryFailedException();
            }
        }



        public async Task<Guid> ValidateOtpAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            // Validate OTP purpose
            if (!Enum.IsDefined(typeof(OtpPurpose), purpose))
                throw new UnsupportedOtpPurposeException();

            OtpEntry? otpEntry = await _otpRepository.GetLatestOtpAsync(email, purpose, cancellationToken);

            // No otp record found
            if (otpEntry == null)
                throw new InvalidOtpException();

            // User entered OTP code does not match
            if (otpEntry.OtpCode != otp)
                throw new InvalidOtpException();

            // Otp already used
            if (otpEntry.IsUsed)
                throw new OtpAlreadyUsedException();

            // Otp expired
            if (otpEntry.ExpiresAt < DateTime.UtcNow)
                throw new OtpExpiredException();

            // All checks passed — mark as used
            await _otpRepository.MarkOtpAsUsedAsync(otpEntry.OtpID, cancellationToken);
            return otpEntry.OtpID;
        }
    }
}
