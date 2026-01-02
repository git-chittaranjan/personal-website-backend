using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using my_api_app.DTOs;
using my_api_app.Models;
using my_api_app.Services;

namespace my_api_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokenService;
        private readonly IOtpService _otpService;

        public AuthController(IConfiguration config, IPasswordHasher hasher, ITokenService tokenService, IOtpService otpService)
        {
            _config = config;
            _hasher = hasher;
            _tokenService = tokenService;
            _otpService = otpService;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB connection string not configured.");

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            using var con = new SqlConnection(Conn);

            // Check if exists
            using (var checkCmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE LOWER(Email)=LOWER(@Email)", con))
            {
                checkCmd.Parameters.AddWithValue("@Email", dto.Email);
                con.Open();

                var result = await checkCmd.ExecuteScalarAsync();
                var exists = result != null && result != DBNull.Value && Convert.ToInt32(result) > 0;

                con.Close();

                if (exists)
                    return BadRequest(new { message = "Email already registered." });
            }

            var (hash, salt) = _hasher.HashPassword(dto.Password);

            using (var insertCmd = new SqlCommand(@"
                INSERT INTO Users (Name, Email, Gender, PasswordHash, PasswordSalt, IsEmailVerified)
                VALUES (@Name, @Email, @Gender, @Hash, @Salt, 0)", con))
            {
                insertCmd.Parameters.AddWithValue("@Name", dto.Name);
                insertCmd.Parameters.AddWithValue("@Email", dto.Email.ToLower());
                insertCmd.Parameters.AddWithValue("@Gender", dto.Gender ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Hash", hash);
                insertCmd.Parameters.AddWithValue("@Salt", salt);

                con.Open();
                await insertCmd.ExecuteNonQueryAsync();
                con.Close();
            }

            var userID = "BA8EE490-1FE6-4CD1-8D24-02B6334F405B";

            await _otpService.GenerateAndStoreOtpAsync(Guid.Parse(userID), dto.Email, OtpPurpose.EmailVerification);

            return Ok(new { message = "Registered. Verification OTP sent to email." });
        }


        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginRequestDto dto)
        //{
        //    User? user = null;

        //    using (var con = new SqlConnection(Conn))
        //    using (var cmd = new SqlCommand(
        //        "SELECT TOP 1 Id, Name, Email, Gender, PasswordHash, PasswordSalt FROM Users WHERE LOWER(Email)=LOWER(@Email)", con))
        //    {
        //        cmd.Parameters.AddWithValue("@Email", dto.Email);
        //        con.Open();

        //        using var r = await cmd.ExecuteReaderAsync();
        //        if (await r.ReadAsync())
        //        {
        //            user = new User
        //            {
        //                Id = (int)r["Id"],
        //                Name = r["Name"].ToString()!,
        //                Email = r["Email"].ToString()!,
        //                Gender = r["Gender"]?.ToString(),
        //                PasswordHash = (byte[])r["PasswordHash"],
        //                PasswordSalt = (byte[])r["PasswordSalt"]
        //            };
        //        }
        //    }

        //    if (user == null)
        //        return Unauthorized(new { message = "Invalid credentials." });

        //    if (!_hasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
        //        return Unauthorized(new { message = "Invalid credentials." });

        //    // OTP not supplied -> send OTP & return 202
        //    if (string.IsNullOrEmpty(dto.Otp))
        //    {
        //        await _otpService.GenerateAndStoreOtpAsync(user.Email, OtpPurpose.Login);
        //        return StatusCode(202, new { message = "OTP sent to email. Provide OTP to complete login." });
        //    }

        //    // Validate OTP
        //    var ok = await _otpService.ValidateOtpAsync(user.Email, dto.Otp!, OtpPurpose.Login);
        //    if (!ok) return Unauthorized(new { message = "Invalid or expired OTP." });

        //    // Generate tokens
        //    var accessToken = _tokenService.GenerateAccessToken(user);
        //    var (refreshToken, refreshExpiry) = _tokenService.GenerateRefreshToken();

        //    using (var con = new SqlConnection(Conn))
        //    using (var cmd = new SqlCommand(@"
        //        INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, IsRevoked)
        //        VALUES (@UserId, @Token, @ExpiresAt, 0)", con))
        //    {
        //        cmd.Parameters.AddWithValue("@UserId", user.Id);
        //        cmd.Parameters.AddWithValue("@Token", refreshToken);
        //        cmd.Parameters.AddWithValue("@ExpiresAt", refreshExpiry);

        //        con.Open();
        //        await cmd.ExecuteNonQueryAsync();
        //    }

        //    return Ok(new LoginResponseDto
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = refreshToken,
        //        AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
        //        UserId = user.Id,
        //        Email = user.Email,
        //        Name = user.Name
        //    });
        //}

        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> Refresh(RefreshRequestDto dto)
        //{
        //    RefreshToken? existing = null;
        //    User? user = null;

        //    using (var con = new SqlConnection(Conn))
        //    using (var cmd = new SqlCommand(@"
        //        SELECT TOP 1 r.Id, r.UserId, r.ExpiresAt, r.IsRevoked,
        //                     u.Id AS UId, u.Email, u.Name
        //        FROM RefreshTokens r
        //        INNER JOIN Users u ON u.Id = r.UserId
        //        WHERE r.Token=@Token", con))
        //    {
        //        cmd.Parameters.AddWithValue("@Token", dto.RefreshToken);
        //        con.Open();

        //        using var r = await cmd.ExecuteReaderAsync();
        //        if (await r.ReadAsync())
        //        {
        //            existing = new RefreshToken
        //            {
        //                Id = (int)r["Id"],
        //                UserId = (int)r["UserId"],
        //                ExpiresAt = (DateTime)r["ExpiresAt"],
        //                IsRevoked = (bool)r["IsRevoked"]
        //            };

        //            user = new User
        //            {
        //                Id = (int)r["UId"],
        //                Email = r["Email"].ToString()!,
        //                Name = r["Name"].ToString()!
        //            };
        //        }
        //    }

        //    if (existing == null || user == null)
        //        return Unauthorized(new { message = "Invalid refresh token." });

        //    if (existing.IsRevoked || existing.ExpiresAt <= DateTime.UtcNow)
        //        return Unauthorized(new { message = "Refresh token expired or revoked." });

        //    // rotate token
        //    using (var con = new SqlConnection(Conn))
        //    {
        //        await con.OpenAsync();
        //        using var tx = con.BeginTransaction();

        //        try
        //        {
        //            using (var revoke = new SqlCommand(
        //                "UPDATE RefreshTokens SET IsRevoked=1 WHERE Id=@Id", con, tx))
        //            {
        //                revoke.Parameters.AddWithValue("@Id", existing.Id);
        //                await revoke.ExecuteNonQueryAsync();
        //            }

        //            var (newToken, newExp) = _tokenService.GenerateRefreshToken();

        //            using (var insert = new SqlCommand(@"
        //                INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, IsRevoked)
        //                VALUES (@UserId, @Token, @ExpiresAt, 0)", con, tx))
        //            {
        //                insert.Parameters.AddWithValue("@UserId", user.Id);
        //                insert.Parameters.AddWithValue("@Token", newToken);
        //                insert.Parameters.AddWithValue("@ExpiresAt", newExp);
        //                await insert.ExecuteNonQueryAsync();
        //            }

        //            tx.Commit();

        //            var newAccess = _tokenService.GenerateAccessToken(user);

        //            return Ok(new LoginResponseDto
        //            {
        //                AccessToken = newAccess,
        //                RefreshToken = newToken,
        //                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
        //                UserId = user.Id,
        //                Email = user.Email,
        //                Name = user.Name
        //            });
        //        }
        //        catch
        //        {
        //            tx.Rollback();
        //            throw;
        //        }
        //    }
        //}

        //[Authorize]
        //[HttpPost("logout")]
        //public async Task<IActionResult> Logout(RefreshRequestDto dto)
        //{
        //    using var con = new SqlConnection(Conn);
        //    using var cmd = new SqlCommand(
        //        "UPDATE RefreshTokens SET IsRevoked=1 WHERE Token=@Token", con);

        //    cmd.Parameters.AddWithValue("@Token", dto.RefreshToken);

        //    con.Open();
        //    await cmd.ExecuteNonQueryAsync();

        //    return Ok(new { message = "Logged out." });
        //}


    }
}
