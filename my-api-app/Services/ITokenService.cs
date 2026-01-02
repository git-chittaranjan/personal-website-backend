using System;
using my_api_app.Models;
using System.Security.Claims;

namespace my_api_app.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        (string token, DateTime expiresAt) GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
