
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
        private readonly byte[] _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenMinutes;

        public JwtTokenService(IConfiguration config)
        {
            var jwtKey = config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new ArgumentNullException("Jwt:Key is missing from configuration.");

            var issuer = config["Jwt:Issuer"];
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentNullException("Jwt:Issuer is missing from configuration.");

            var audience = config["Jwt:Audience"];
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentNullException("Jwt:Audience is missing from configuration.");

            if (!int.TryParse(config["Jwt:AccessTokenExpiryMinutes"], out var accessTokenMinutes))
                throw new ArgumentNullException("Jwt:AccessTokenExpiryMinutes is missing in configuration.");

            _key = Encoding.UTF8.GetBytes(jwtKey);
            _issuer = issuer;
            _audience = audience;
            _accessTokenMinutes = accessTokenMinutes;
        }

        public UserLoginResponseDto GenerateAccessToken(User user, JwtTokenPurpose tokenPurpose)
        {
            var now = DateTimeOffset.UtcNow;
            var expiry = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

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
                new Claim("mfa", true.ToString()),
                new Claim("purpose", tokenPurpose.ToString())
            };

            var key = new SymmetricSecurityKey(_key);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

            var loginResponse = new UserLoginResponseDto
            {
                UserId = user.UserID,
                Email = user.Email,
                Name = user.Name,
                AccessToken = serializedToken,
                ExpiresAt = expiry,
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
    }
}
