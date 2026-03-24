using my_api_app.Core.Exceptions.BusinessExceptions;
using my_api_app.Core.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Domain.Enums;
using System.Net;
using System.Net.Mail;

namespace my_api_app.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, IWebHostEnvironment env, ILogger<EmailService> logger)
        {
            _config = config;
            _env = env;
            _logger = logger;
        }



        private static string GetEmailPurposeText(OtpPurpose purpose)
        {
            return purpose switch
            {
                OtpPurpose.EMAIL_VERIFICATION => "Email",
                OtpPurpose.LOGIN => "Login",
                OtpPurpose.PASSWORD_RESET => "Password Reset",
                _ => ""
            };
        }



        public async Task SendEmailAsync(string name, string to, string otpCode, int expiryMinutes, OtpPurpose purpose)
        {
            _logger.LogInformation("SendEmailAsync - Preparing to send {Purpose} OTP email to {Email}", purpose, to);

            var smtpHost = _config["Email:SmtpHost"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:SmtpUser"];
            var pass = _config["Email:SmtpPass"];
            var from = _config["Email:From"];
            var emailSubject = _config["Email:OtpSubject"];

            //Validate Configurations
            if (string.IsNullOrEmpty(smtpHost) ||
                 string.IsNullOrEmpty(user) ||
                 string.IsNullOrEmpty(pass) ||
                 string.IsNullOrEmpty(from) ||
                 string.IsNullOrEmpty(emailSubject))
            {
                throw new ConfigurationException(
                                configKey: "Email:SmtpHost, Email:SmtpPort, Email:SmtpUser, Email:SmtpPass, Email:From, Email:OtpSubject",
                                detail: "One or more of the above-mentioned configuration values are missing in appsettings."
                            ); //Middleware will catch and log this.
            }

            //Getting the HTML template
            var templatePath = Path.Combine(_env.ContentRootPath, "Infrastructure", "Email", "EmailTemplates", "OtpTemplate.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("SendEmailAsync - Email template not found at path: {TemplatePath}", templatePath);

                throw new ConfigurationException(
                                configKey: "Email Template Path",
                                detail: "Cannot find the email template at the specified path."
                            );
            }

            var purposeText = GetEmailPurposeText(purpose);

            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{NAME}}", name)
                       .Replace("{{OTP_CODE}}", otpCode)
                       .Replace("{{OTP_PURPOSE}}", purposeText)
                       .Replace("{{EXP_MINUTES}}", expiryMinutes.ToString());

            //HTML enabled for OTPs
            var mail = new MailMessage(from, to, emailSubject, body)
            {
                IsBodyHtml = true
            };


            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                Timeout = 50000
            };

            try
            {
                await client.SendMailAsync(mail);

                _logger.LogInformation("SendEmailAsync - OTP email sent successfully to {Email} for purpose {Purpose}", to, purpose);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SendEmailAsync - SMTP failure while sending {Purpose} OTP to {Email} — StatusCode: {StatusCode}", purpose, to, ex.StatusCode);

                throw new SmtpServiceUnavailableException(); // SMTP server issue
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendEmailAsync - Unexpected failure while sending {Purpose} OTP to {Email}", purpose, to);

                throw new InternalServerException(); // Unexpected failure
            }
        }
    }
}
