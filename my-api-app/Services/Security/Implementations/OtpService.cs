using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using my_api_app.Enums;
using my_api_app.Helpers;
using my_api_app.Models;
using my_api_app.Repositories.Interfaces;
using my_api_app.Services.Security.Interfaces;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
            await _emailService.SendEmailAsync(name, email, otp, expiryMinutes);
        }



        public async Task<bool> ValidateOtpAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            Guid? otpId = await _otpRepository.GetValidOtpIdAsync(email, otp, purpose, cancellationToken);

            if (!otpId.HasValue)
                return false;

            await _otpRepository.MarkOtpAsUsedAsync(otpId.Value, cancellationToken);

            return true;
        }
    }
}
