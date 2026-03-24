using Microsoft.Data.SqlClient;
using my_api_app.Core.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.Infrastructure.Database;
using System.Data;

namespace my_api_app.Repositories.User
{
    public class PendingUserRepository : IPendingUserRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PendingUserRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }



        public async Task<bool> CreatePendingUserAsync(Domain.Models.User pendingUser, CancellationToken cancellationToken)
        {
            if (pendingUser is null)
                throw new ArgumentNullException(nameof(pendingUser));

            const string sql = "INSERT INTO PendingUsers (Name, Gender, Email, PasswordHash, PasswordSalt, IsEmailVerified) VALUES (@Name, @Gender, @Email, @PasswordHash, @PasswordSalt, @IsEmailVerified);";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 200).Value = pendingUser.Name;
            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 200).Value = pendingUser.Email;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 64).Value = pendingUser.PasswordHash;
            cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 64).Value = pendingUser.PasswordSalt;
            cmd.Parameters.Add("@IsEmailVerified", SqlDbType.Bit).Value = pendingUser.IsEmailVerified;
            cmd.Parameters.Add("@Gender", SqlDbType.NVarChar, 10).Value = pendingUser.Gender.HasValue
                                    ? (object)pendingUser.Gender.Value.ToString() : DBNull.Value;

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }



        public async Task<Domain.Models.User?> GetPendingUserAsync(string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must not be empty.", nameof(email));

            const string sql = "SELECT TOP 1 Name, Gender, Email, PasswordHash, PasswordSalt FROM PendingUsers WHERE Email = @Email AND ExpiresAt > SYSUTCDATETIME() ORDER BY CreatedAt DESC;";


            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = email;

            await con.OpenAsync(cancellationToken);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            // Gender is String in DB where as in User model it is Enum 
            Gender? gender = null;
            int genderOrdinal = reader.GetOrdinal("Gender"); //Reurns column position

            if (!reader.IsDBNull(genderOrdinal))
            {
                var genderStr = reader.GetString(genderOrdinal);

                if (!Enum.TryParse<Gender>(genderStr, true, out var parsedGender))
                {
                    throw new InternalServerException();
                }

                gender = parsedGender;
            }

            return new Domain.Models.User
            {
                Name = reader.GetString(0),
                Gender = gender,
                Email = reader.GetString(2),
                PasswordHash = (byte[])reader["PasswordHash"],
                PasswordSalt = (byte[])reader["PasswordSalt"],
                IsEmailVerified = false
            };
        }



        public async Task<bool> DeletePendingUserAsync(string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must not be empty.", nameof(email));

            const string sql = "DELETE FROM PendingUsers WHERE Email = @Email";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }
    }
}
