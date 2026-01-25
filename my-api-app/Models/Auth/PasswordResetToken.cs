namespace my_api_app.Models.Auth
{
    public class PasswordResetToken
    {
        public Guid TokenID { get; set; }
        public string Email { get; set; } = default!;
        public byte[] TokenHash { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
