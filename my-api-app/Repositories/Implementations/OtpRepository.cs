using Microsoft.Data.SqlClient;
using my_api_app.Data;
using my_api_app.Enums;
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



        public async Task<Guid?> GetValidOtpIdAsync(string email, string otp, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            const string sql1 = "SELECT TOP 1 OtpID FROM OtpEntries WITH (UPDLOCK, ROWLOCK) WHERE Email = @EmailID AND OtpPurpose = @OtpPurpose AND IsUsed = 0 AND ExpiresAt >= SYSUTCDATETIME() AND OtpCode = @OtpCode ORDER BY CreatedAt DESC;";
            const string sql2 = "UPDATE OtpEntries SET IsUsed = 1 WHERE OtpID = @OtpID;";


            using SqlConnection con = _factory.CreateConnection();
            await con.OpenAsync(cancellationToken);

            using var tx = con.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                using var cmd = new SqlCommand(sql1, con, tx);

                cmd.Parameters.AddWithValue("@EmailID", email);
                cmd.Parameters.AddWithValue("@OtpPurpose", purpose.ToString());
                cmd.Parameters.AddWithValue("@OtpCode", otp);

                var result = await cmd.ExecuteScalarAsync(cancellationToken);

                if (result == null)
                {
                    tx.Rollback();
                    return null;
                }

                Guid otpId = (Guid)result;

                using var cmd2 = new SqlCommand(sql2, con, tx);
                cmd2.Parameters.AddWithValue("@OtpID", otpId);
                await cmd2.ExecuteNonQueryAsync(cancellationToken);

                tx.Commit();
                return otpId;
            }
            catch
            {
                tx.Rollback();
                throw new InvalidDataException($"Invalid Operation"); ;
            }
        }



        public async Task<bool> MarkOtpAsUsedAsync(Guid otpId, CancellationToken cancellationToken)
        {
            // Already handled inside GetValidOtpIdAsync transactional flow above

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
