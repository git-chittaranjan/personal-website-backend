using my_api_app.Enums;

namespace my_api_app.Models.Auth
{
    public class OtpEntry
    {
        public Guid OtpID { get; set; }
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public OtpPurpose OtpPurpose { get; set; }
    }
}


