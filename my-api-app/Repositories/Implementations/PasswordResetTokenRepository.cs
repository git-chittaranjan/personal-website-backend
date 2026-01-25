using Microsoft.Data.SqlClient;
using my_api_app.Data;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;
using System.Data;

namespace my_api_app.Repositories.Implementations
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PasswordResetTokenRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }



        public async Task<bool> CreateResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken)
        {
            const string sql = "INSERT INTO PasswordResetTokens (Email, TokenHash, ExpiresAt, IsUsed) VALUES (@Email, @TokenHash, @ExpiresAt, 0);";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = token.Email;
            cmd.Parameters.Add("@TokenHash", SqlDbType.VarBinary, 64).Value = token.TokenHash;
            cmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = token.ExpiresAt;

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }



        public async Task<PasswordResetToken?> GetResetTokenAsync(byte[] tokenHash, CancellationToken cancellationToken)
        {
            const string sql = "SELECT TokenID, Email, ExpiresAt, IsUsed FROM PasswordResetTokens WHERE TokenHash = @TokenHash AND IsUsed = 0 AND ExpiresAt > SYSUTCDATETIME();";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@TokenHash", SqlDbType.VarBinary, 64).Value = tokenHash;

            await con.OpenAsync(cancellationToken);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new PasswordResetToken
            {
                TokenID = reader.GetGuid(0),
                Email = reader.GetString(1),
                ExpiresAt = reader.GetDateTime(2),
                IsUsed = reader.GetBoolean(3)
            };
        }



        public async Task<bool> MarkAsUsedResetTokenAsync(Guid tokenId, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE PasswordResetTokens SET IsUsed = 1 WHERE TokenID = @TokenID;";

            using var con = _factory.CreateConnection();
            using var cmd = new SqlCommand(sql , con);

            cmd.Parameters.Add("@TokenID", SqlDbType.UniqueIdentifier).Value = tokenId;

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }
    }
}
