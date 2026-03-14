using my_api_app.Enums;

namespace my_api_app.DTOs.User
{
    public class UpdateUserRequestDto
    {
        public string Name { get; set; } = default!;
        public Gender? Gender { get; set; }
    }
}
