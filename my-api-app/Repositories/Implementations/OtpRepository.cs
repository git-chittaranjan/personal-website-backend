using Microsoft.Data.SqlClient;
using my_api_app.Data;
using my_api_app.Enums;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;
using System.Data;

namespace my_api_app.Repositories.Implementations
{
    public class OtpRepository : IOtpRepository
    {
        private readonly IDbConnectionFactory _factory;

        public OtpRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> SaveOtpAsync(string email, string otp, DateTime expiresAt, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            const string sql = "INSERT INTO OtpEntries (Email, OtpCode, ExpiresAt, IsUsed, CreatedAt, OtpPurpose) VALUES (@Email, @OtpCode, @ExpiresAt, 0, SYSUTCDATETIME(), @Purpose);";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@OtpCode", otp);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            cmd.Parameters.AddWithValue("@Purpose", purpose.ToString());

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }



        public async Task<OtpEntry?> GetLatestOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT TOP 1 OtpID, OtpCode, IsUsed, ExpiresAt FROM OtpEntries WITH (UPDLOCK, ROWLOCK) WHERE Email = @Email AND OtpPurpose = @OtpPurpose ORDER BY CreatedAt DESC;";

            using SqlConnection con = _factory.CreateConnection();
            await con.OpenAsync(cancellationToken);

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@OtpPurpose", purpose.ToString());

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new OtpEntry
                {
                    OtpID = reader.GetGuid(0),
                    OtpCode = reader.GetString(1),
                    IsUsed = reader.GetBoolean(2),
                    ExpiresAt = reader.GetDateTime(3)
                };
            }

            return null;
        }



        public async Task<bool> MarkOtpAsUsedAsync(Guid otpId, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE OtpEntries SET IsUsed = 1 WHERE OtpID = @OtpID;";

            using SqlConnection con = _factory.CreateConnection();
            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@OtpID", otpId);

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }
    }
}
