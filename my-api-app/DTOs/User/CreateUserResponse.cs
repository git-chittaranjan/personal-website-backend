namespace my_api_app.DTOs.User
{
    public class CreateUserResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
