using System;
using System.Collections.Generic;

namespace my_api_app.Models
{
    public class User
    {
        public Guid UserID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string Gender { get; set; } = default!;
        public string Email { get; set; } = default!;
        public byte[] PasswordHash { get; set; } = default!;
        public byte[] PasswordSalt { get; set; } = default!;
        public bool IsEmailVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
