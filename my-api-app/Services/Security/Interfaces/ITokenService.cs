using System;
using System.Security.Claims;
using my_api_app.DTOs.Auth.Login;
using my_api_app.Enums;
using my_api_app.Models.Auth;

namespace my_api_app.Services.Security.Interfaces
{
    public interface ITokenService
    {
        UserLoginResponseDto? GenerateAccessToken(User user, JwtTokenPurpose tokenPurpose);
        (string token, DateTime expiresAt) GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        JwtUserClaims? GetTokenClaims(ClaimsPrincipal userPrincipal);
    }
}
