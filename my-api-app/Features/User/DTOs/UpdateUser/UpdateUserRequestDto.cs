using my_api_app.Domain.Enums;

namespace my_api_app.Features.User.DTOs.UpdateUser
{
    public class UpdateUserRequestDto
    {
        public string Name { get; set; } = default!;
        public Gender? Gender { get; set; }
    }
}
