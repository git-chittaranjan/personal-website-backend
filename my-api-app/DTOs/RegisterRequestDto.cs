using System;

namespace my_api_app.DTOs
{
    public class RegisterRequestDto
    {
        public string Name { get; set; } = default!;
        public string Gender { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}

