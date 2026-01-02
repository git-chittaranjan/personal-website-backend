using System;

namespace my_api_app.Models
{
    public class OtpEntry
    {
        public Guid OtpID { get; set; } = Guid.NewGuid();
        public Guid UserID { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OtpPurpose OtpPurpose { get; set; }
    }
    public enum OtpPurpose : byte
    {
        Login = 3,
        EmailVerification = 1,
        PasswordReset = 2        
    }
}


