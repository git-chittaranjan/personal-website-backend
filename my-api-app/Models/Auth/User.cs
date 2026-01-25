using my_api_app.Enums;

namespace my_api_app.Models.Auth
{
    public class User
    {
        public Guid UserID { get; set; }
        public string? Name { get; set; } //Nullable
        public Gender? Gender { get; set; } = default!; //Nullable
        public string Email { get; set; } = default!;
        public byte[] PasswordHash { get; set; } = default!;
        public byte[] PasswordSalt { get; set; } = default!;
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActice { get; set; } = true;
    }
}
