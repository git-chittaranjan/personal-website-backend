using System.Net;
using System.Net.Mail;
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
            if (string.IsNullOrEmpty(smtpHost))
                throw new InvalidOperationException("SMTP host missing");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                throw new InvalidOperationException("SMTP credentials missing");

            if (string.IsNullOrEmpty(from))
                throw new InvalidOperationException("From address missing");

            if (string.IsNullOrEmpty(emailSubject))
                throw new InvalidOperationException("Email Subject is missing");


            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                Timeout = 50000  // 30s timeout for production
            };

            //Getting the HTML template
            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", "OtpTemplate.html");

            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{NAME}}", name)
                       .Replace("{{OTP_CODE}}", otpCode)
                       .Replace("{{EXP_MINUTES}}", expiryMinutes.ToString());


            //HTML enabled for OTPs
            var mail = new MailMessage(from, to, emailSubject, body)
            {
                IsBodyHtml = true
            };

            try
            {
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to send OTP email.", ex);
            }
        }
    }
}
