using System;
using my_api_app.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace my_api_app.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly byte[] _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenMinutes;
        private readonly int _refreshTokenDays;

        public TokenService(IConfiguration config)
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
            _refreshTokenDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("UserID", user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string token, DateTime expiresAt) GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);
            var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays);
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
