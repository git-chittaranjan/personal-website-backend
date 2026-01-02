using FluentValidation;
using my_api_app.DTOs;

namespace my_api_app.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Gender).NotEmpty().Must(g => g == "Male" || g == "Female" || g == "Other");
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Password confirmation doesn't match.");
        }
    }
}

