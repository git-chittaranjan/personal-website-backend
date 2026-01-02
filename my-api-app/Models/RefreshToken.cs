using System;

namespace my_api_app.Models
{
    public class RefreshToken
    {
        public Guid TokenID { get; set; } = Guid.NewGuid();
        public Guid UserID { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
    }
}
