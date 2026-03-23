using System;
using System.Security.Claims;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.Features.Auth.DTOs.Login;

namespace my_api_app.Infrastructure.Security.Token
{
    public interface IJwtTokenService
    {
        UserLoginResponseDto? GenerateAccessToken(User user, JwtTokenPurpose tokenPurpose);
        JwtUserClaims? GetTokenClaims(ClaimsPrincipal userPrincipal);
    }
}
