using System;
using FluentValidation;
using my_api_app.DTOs.Auth.Login;

namespace my_api_app.Validators.Auth.Login
{
    public class LoginDtoValidator : AbstractValidator<UserLoginRequestDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}

