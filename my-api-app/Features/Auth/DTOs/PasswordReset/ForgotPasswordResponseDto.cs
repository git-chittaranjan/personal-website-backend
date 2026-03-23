namespace my_api_app.DTOs.Auth
{
    public class ForgotPasswordResponseDto
    {
        public string ResetToken { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public string TokenType { get; set; } = "One-Time-Token";
    }
}
