using my_api_app.Enums;

namespace my_api_app.DTOs.User
{
    public class PatchUserRequestDto
    {
        public string? Name { get; set; }
        public Gender? Gender { get; set; }
    }
}
