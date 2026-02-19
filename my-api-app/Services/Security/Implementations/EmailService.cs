using System.Net;
using System.Net.Mail;
using my_api_app.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Services.Security.Interfaces;

namespace my_api_app.Services.Security.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }



        public async Task SendEmailAsync(string name, string to, string otpCode, int expiryMinutes)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:SmtpUser"];
            var pass = _config["Email:SmtpPass"];
            var from = _config["Email:From"];
            var emailSubject = _config["Email:OtpSubject"];

            //Added null checks
            if (string.IsNullOrEmpty(smtpHost) ||
                 string.IsNullOrEmpty(user) ||
                 string.IsNullOrEmpty(pass) ||
                 string.IsNullOrEmpty(from) ||
                 string.IsNullOrEmpty(emailSubject))
            {
                throw new InternalServerException(); // Config issue is a server error, don't expose details
            }

            //Getting the HTML template
            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "OtpTemplate.html");
            if (!File.Exists(templatePath))
            {
                throw new InternalServerException();
            }
            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{NAME}}", name)
                       .Replace("{{OTP_CODE}}", otpCode)
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
            }
            catch (SmtpException)
            {
                throw new SmtpServiceUnavailableException(); // SMTP server issue
            }
            catch (Exception)
            {
                throw new InternalServerException(); // Unexpected failure
            }
        }
    }
}
