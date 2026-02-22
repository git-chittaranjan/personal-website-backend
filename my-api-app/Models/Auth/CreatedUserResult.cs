namespace my_api_app.Models.Auth
{
    public sealed class CreatedUserResult
    {
        public Guid UserID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
