namespace my_api_app.Features.User.DTOs.CreateUser
{
    public class CreateUserResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
