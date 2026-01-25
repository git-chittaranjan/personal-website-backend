using FluentValidation;
using my_api_app.DTOs.Auth;

namespace my_api_app.Validators.Auth
{
    public class VerifyOtpValidator : AbstractValidator<VerifyOtpRequestDto>
    {
        public VerifyOtpValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP length must be of 6 digits")
                .Matches("^[0-9]+$").WithMessage("OTP must contain only digits");

            RuleFor(x => x.OtpPurpose)
                .NotNull().WithMessage("OTP purpose is required")
                .IsInEnum().WithMessage("OtpPurpose must be a value between 1 to 3");
        }
    }
}
