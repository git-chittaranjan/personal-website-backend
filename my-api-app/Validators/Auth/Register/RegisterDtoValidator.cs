using FluentValidation;
using my_api_app.DTOs.Auth.Register;

namespace my_api_app.Validators.Auth.Register
{
    public class RegisterDtoValidator : AbstractValidator<UserRegisterRequestDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.Gender).IsInEnum()
                .When(x => x.Gender.HasValue).WithMessage("Gender must be a value between 1 to 3");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

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

