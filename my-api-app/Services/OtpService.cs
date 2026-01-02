using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using my_api_app.Models;

namespace my_api_app.Services
{
    public class OtpService : IOtpService
    {
        private readonly string _connectionString;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;

        public OtpService(IConfiguration config, IEmailService email)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _connectionString = _config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DB Connection string not found.");
        }

        public async Task<string> GenerateAndStoreOtpAsync(Guid userId, string email, OtpPurpose purpose)
        {
            //Generates six digit OTP code
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var code = BitConverter.ToUInt32(bytes, 0) % 1000000;
            var codeStr = code.ToString("D6");

            var expiryMinutes = int.Parse(_config["Otp:ExpiryMinutes"] ?? "5");

            //Database connection
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO OtpEntries 
                (UserID, OtpCode, ExpiresAt, IsUsed, CreatedAt, OtpPurpose)
                VALUES (@UserID, @OtpCode, @ExpiresAt, 0, SYSUTCDATETIME(), @OtpPurpose);
            ", conn))
            {
                cmd.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = userId;
                cmd.Parameters.Add("@OtpCode", SqlDbType.NVarChar, 20).Value = codeStr;
                cmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = DateTime.UtcNow.AddMinutes(expiryMinutes);
                cmd.Parameters.Add("@OtpPurpose", SqlDbType.TinyInt).Value = (byte)purpose;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            await _email.SendEmailAsync(email, codeStr, expiryMinutes);

            return codeStr;
        }

        public async Task<bool> ValidateOtpAsync(Guid userId, string email, string otp, OtpPurpose purpose)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    Guid? otpId = null;

                    // 1. Fetch latest valid OTP — locked to prevent race conditions
                    using (var cmd = new SqlCommand(@"
                        SELECT TOP 1 OtpID
                        FROM OtpEntries WITH (UPDLOCK, ROWLOCK)
                        WHERE UserID = @UserID
                          AND OtpPurpose = @OtpPurpose
                          AND IsUsed = 0
                          AND ExpiresAt >= SYSUTCDATETIME()
                          AND OtpCode = @OtpCode
                        ORDER BY CreatedAt DESC;
                    ", conn, tx))
                    {
                        cmd.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = userId;
                        cmd.Parameters.Add("@OtpPurpose", SqlDbType.TinyInt).Value = (byte)purpose;
                        cmd.Parameters.Add("@OtpCode", SqlDbType.NVarChar, 20).Value = otp;

                        var result = await cmd.ExecuteScalarAsync();

                        if (result == null)
                        {
                            tx.Rollback();
                            return false;
                        }

                        otpId = (Guid)result;
                    }

                    // 2. Mark OTP as used
                    using (var update = new SqlCommand(@"
                        UPDATE OtpEntries
                        SET IsUsed = 1
                        WHERE OtpID = @OtpID;
                    ", conn, tx))
                    {
                        update.Parameters.Add("@OtpID", SqlDbType.UniqueIdentifier).Value = otpId.Value;
                        await update.ExecuteNonQueryAsync();
                    }

                    tx.Commit();
                    return true;
                }
            }
        }
    }
}
