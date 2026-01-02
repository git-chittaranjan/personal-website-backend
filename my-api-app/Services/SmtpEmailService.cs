using System.Net;
using System.Net.Mail;
using System.IO;

namespace my_api_app.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string otpCode, int expiryMinutes)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:SmtpUser"];
            var pass = _config["Email:SmtpPass"];
            var from = _config["Email:From"];

            //Added null checks
            if (string.IsNullOrEmpty(smtpHost))
                throw new InvalidOperationException("SMTP host missing");

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                throw new InvalidOperationException("SMTP credentials missing");

            if (string.IsNullOrEmpty(from))
                throw new InvalidOperationException("From address missing");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass),
                Timeout = 30000  // 30s timeout for production
            };

            //Getting the HTML template
            var templatePath = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "OtpTemplate.html");
            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{OTP_CODE}}", otpCode)
                       .Replace("{{EXP_MINUTES}}", expiryMinutes.ToString());
            var subject = "Your OTP Code";


            //HTML enabled for OTPs
            var mail = new MailMessage(from, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
