namespace my_api_app.DTOs.Auth.Register
{
    public class UserRegisterResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
