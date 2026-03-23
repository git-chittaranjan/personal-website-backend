using my_api_app.Domain.Enums;

namespace my_api_app.Features.User.DTOs.PatchUser
{
    public class PatchUserRequestDto
    {
        public string? Name { get; set; }
        public Gender? Gender { get; set; }
    }
}
