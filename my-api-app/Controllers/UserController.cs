using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_api_app.DTOs;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace my_api_app.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        public UserController(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("DB connection string not configured.");

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var uid = User.FindFirstValue("UserID");
            if (uid == null) return Unauthorized();

            using var con = new SqlConnection(Conn);
            using var cmd = new SqlCommand(
                "SELECT Name, Gender, Email, IsEmailVerified, CreatedAt FROM Users WHERE UserID = @Id", con);

            cmd.Parameters.AddWithValue("@Id", uid);

            con.Open();
            using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync()) return NotFound();

            var profile = new UserProfileResponseDto
            {
                UserID = (Guid)r["UserID"],
                Name = r["Name"]?.ToString(),
                Email = r["Email"].ToString()!,
                Gender = r["Gender"]?.ToString(),
                EmailVerified = (bool)r["IsEmailVerified"],
                CreatedAt = (DateTime)r["CreatedAt"]
            };

            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequestDto dto)
        {
            var uid = User.FindFirstValue("UserID");
            if (uid == null) return Unauthorized();

            using var con = new SqlConnection(Conn);
            using var cmd = new SqlCommand(@"
               UPDATE Users SET 
                Name = ISNULL(@Name, Name),
                Gender = ISNULL(@Gender, Gender)
               WHERE UserID = @Id", con);

            cmd.Parameters.AddWithValue("@Id", uid);
            cmd.Parameters.AddWithValue("@Name", (object?)dto.Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", (object?)dto.Gender ?? DBNull.Value);

            con.Open();
            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0 ? Ok(new { message = "Profile updated." }) : NotFound();
        }
    }
}
