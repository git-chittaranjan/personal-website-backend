using System;
using my_api_app.DTOs;
using FluentValidation;

namespace my_api_app.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}

