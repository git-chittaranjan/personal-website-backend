using System;

namespace my_api_app.DTOs.Auth.Login
{
    public class UserLoginResponseDto
    {
        public Guid? UserId { get; set; }
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
        public DateTime? ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}

