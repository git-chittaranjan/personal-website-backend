using System;

namespace my_api_app.DTOs
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? Otp { get; set; } // optional: if not provided, server will send OTP
    }
}

