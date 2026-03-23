namespace my_api_app.Features.Auth.DTOs.Login
{
    public class UserLoginRequestDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
