using FluentValidation;
using my_api_app.DTOs.User;

namespace my_api_app.Validators.User
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required").IsInEnum()
                .When(x => x.Gender.HasValue).WithMessage("Gender must be a value between 1 to 3");
        }
    }
}
