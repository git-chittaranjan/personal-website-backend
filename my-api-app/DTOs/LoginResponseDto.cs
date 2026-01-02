using System;

namespace my_api_app.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime AccessTokenExpiry { get; set; }
        public Guid UserID { get; set; }
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}

