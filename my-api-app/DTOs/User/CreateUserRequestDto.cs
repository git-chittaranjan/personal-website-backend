using my_api_app.Enums;

namespace my_api_app.DTOs.User
{
    public class CreateUserRequestDto
    {
        public string Name { get; set; } = default!;
        public Gender? Gender { get; set; } //Nullable
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
