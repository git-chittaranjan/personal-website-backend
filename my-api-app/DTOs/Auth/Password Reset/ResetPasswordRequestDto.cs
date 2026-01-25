namespace my_api_app.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        public string Email { get; set; } = default!;
        public string ResetToken { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
