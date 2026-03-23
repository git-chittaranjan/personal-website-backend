using my_api_app.Domain.Enums;

namespace my_api_app.Features.Auth.DTOs.Register
{
    public class UserRegisterRequestDto
    {
        public string Name { get; set; } = default!;
        public Gender? Gender { get; set; } //Nullable
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}

