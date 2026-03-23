using my_api_app.Domain.Enums;

namespace my_api_app.Domain.Models
{
    public class OtpEntry
    {
        public Guid OtpID { get; set; }
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public OtpPurpose OtpPurpose { get; set; }
    }
}


