using FluentValidation;
using my_api_app.DTOs.User;

namespace my_api_app.Validators.User
{
    public class PatchUserDtoValidator : AbstractValidator<PatchUserRequestDto>
    {
        public PatchUserDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Gender).IsInEnum()
                .When(x => x.Gender.HasValue).WithMessage("Gender must be a value between 1 to 3");
        }
    }
}
