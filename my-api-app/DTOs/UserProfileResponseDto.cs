namespace my_api_app.DTOs
{
    public class UserProfileResponseDto
    {
        public Guid UserID { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; } = default!;
        public string? Gender { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
