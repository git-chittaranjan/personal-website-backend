using my_api_app.Domain.Enums;

namespace my_api_app.Domain.Models
{
    public class UserDetails
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; } = default!;
        public Gender? Gender { get; set; }
        public string Email { get; set; } = default!;
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
