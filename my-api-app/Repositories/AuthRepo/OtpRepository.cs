using Microsoft.Data.SqlClient;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.Infrastructure.Database;
using System.Data;

namespace my_api_app.Repositories.Auth
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

            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 200).Value = email;
            cmd.Parameters.Add("@OtpCode", SqlDbType.VarChar, 200).Value = otp;
            cmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = expiresAt; // ← DateTime2 for precision
            cmd.Parameters.Add("@Purpose", SqlDbType.VarChar, 200).Value = purpose.ToString();

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }



        public async Task<OtpEntry?> GetLatestOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            const string sql = @"SELECT TOP 1 OtpID, OtpCode, ExpiresAt, IsUsed, CreatedAt, OtpPurpose FROM OtpEntries WITH (ROWLOCK) WHERE Email = @Email AND OtpPurpose = @OtpPurpose ORDER BY CreatedAt DESC;";

            using SqlConnection con = _factory.CreateConnection();
            await con.OpenAsync(cancellationToken);

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 256).Value = email;
            cmd.Parameters.Add("@OtpPurpose", SqlDbType.VarChar, 50).Value = purpose.ToString();

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new OtpEntry
                {
                    OtpID = reader.GetGuid(reader.GetOrdinal("OtpID")),
                    OtpCode = reader.GetString(reader.GetOrdinal("OtpCode")),
                    ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                    IsUsed = reader.GetBoolean(reader.GetOrdinal("IsUsed")),                    
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    OtpPurpose = Enum.Parse<OtpPurpose>(reader.GetString(reader.GetOrdinal("OtpPurpose")))
                };
            }

            return null;
        }



        public async Task<bool> MarkOtpAsUsedAsync(Guid otpId, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE OtpEntries SET IsUsed = 1 WHERE OtpID = @OtpID;";

            using SqlConnection con = _factory.CreateConnection();
            using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@OtpID", SqlDbType.UniqueIdentifier).Value = otpId;

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }
    }
}
