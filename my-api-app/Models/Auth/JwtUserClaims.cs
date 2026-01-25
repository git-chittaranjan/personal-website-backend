namespace my_api_app.Models.Auth
{
    public class JwtUserClaims
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string? Name { get; init; }
        public string TokenID { get; init; } = string.Empty;
        public DateTime IssuedAtUtc { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public bool IsEmailVerified { get; init; }
        public bool IsMfaEnabled { get; init; }
        public string TokenPurpose { get; init; } = string.Empty;
    }
}
