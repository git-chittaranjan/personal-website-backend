using my_api_app.Domain.Enums;

namespace my_api_app.Domain.Models
{
    public class User
    {
        public Guid UserID { get; set; }
        public string Name { get; set; } = default!;
        public Gender? Gender { get; set; } = default!; //Nullable
        public string Email { get; set; } = default!;
        public byte[] PasswordHash { get; set; } = default!;
        public byte[] PasswordSalt { get; set; } = default!;
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActice { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
