using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using my_api_app.DTOs.Auth.Login;
using my_api_app.Enums;
using my_api_app.Models.Auth;
using my_api_app.Services.Security.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace my_api_app.Services.Security.Implementations
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly byte[] _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenMinutes;
        private readonly int _refreshTokenMinutes;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentException("JWT key configuration ('Jwt:Key') is missing or empty.", nameof(config));
            }
            _key = Encoding.UTF8.GetBytes(jwtKey);
            _issuer = _config["Jwt:Issuer"] ?? "api";
            _audience = _config["Jwt:Audience"] ?? "apiUsers";
            _accessTokenMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15");
            _refreshTokenMinutes = int.Parse(_config["RefreshToken:ExpiryMinutes"] ?? "60");
        }

        public UserLoginResponseDto GenerateAccessToken(User user, JwtTokenPurpose tokenPurpose)
        {
            var now = DateTimeOffset.UtcNow;

            var claims = new List<Claim>
            {
                // Identity
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                // Security
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                // MFA
                new Claim("email_verified", true.ToString()),
                new Claim("mfa", "true"),
                new Claim("purpose", tokenPurpose.ToString())
            };

            var key = new SymmetricSecurityKey(_key);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                signingCredentials: creds
            );

            var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

            var loginResponse = new UserLoginResponseDto
            {
                UserId = user.UserID,
                Email = user.Email,
                Name = user.Name,
                AccessToken = serializedToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                TokenType = "Bearer"
            };

            return loginResponse;
        }



        public JwtUserClaims? GetTokenClaims(ClaimsPrincipal userPrincipal)
        {
            if (userPrincipal?.Identity?.IsAuthenticated != true)
                return null;

            var userID = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var name = userPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            var purpose = userPrincipal.FindFirst("purpose")?.Value;

            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(purpose))
                return null;

            return new JwtUserClaims
            {
                UserId = Guid.Parse(userID),
                Email = email,
                Name = name,
                TokenPurpose = purpose
            };
        }



        public (string token, DateTime expiresAt) GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);
            var expiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenMinutes);
            return (token, expiresAt);
        }



        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _audience, // Must match "app.chittaranjansaha.com,postman"
                ValidateIssuer = true,
                ValidIssuer = _issuer, // Must match your API domain
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key), // Signature must match your _key
                ValidateLifetime = false // Allow processing expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is JwtSecurityToken jwt && jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
