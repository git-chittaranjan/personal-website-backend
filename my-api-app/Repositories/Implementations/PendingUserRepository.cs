using Microsoft.Data.SqlClient;
using my_api_app.Data;
using my_api_app.Enums;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;

namespace my_api_app.Repositories.Implementations
{
    public class PendingUserRepository : IPendingUserRepository
    {
        private readonly IDbConnectionFactory _factory;

        public PendingUserRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }



        public async Task<bool> CreatePendingUserAsync(User pendingUser, CancellationToken cancellationToken)
        {
            const string sql = "INSERT INTO PendingUsers (Name, Gender, Email, PasswordHash, PasswordSalt, IsEmailVerified) VALUES (@Name, @Gender, @Email, @PasswordHash, @PasswordSalt, @IsEmailVerified);";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Name", pendingUser.Name);
            cmd.Parameters.AddWithValue("@Gender", pendingUser.Gender.HasValue ? pendingUser.Gender.Value.ToString() : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", pendingUser.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", pendingUser.PasswordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", pendingUser.PasswordSalt);
            cmd.Parameters.AddWithValue("@IsEmailVerified", pendingUser.IsEmailVerified);

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }



        public async Task<User?> GetPendingUserAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = "SELECT TOP 1 Name, Gender, Email, PasswordHash, PasswordSalt FROM PendingUsers WHERE Email = @Email AND ExpiresdAt > SYSUTCDATETIME() ORDER BY CreatedAt DESC;";


            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);

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
                    throw new InvalidDataException($"Invalid Gender value '{genderStr}' in DB");
                }

                gender = parsedGender;
            }

            return new User
            {
                Name = reader.GetString(0),
                Gender = gender,
                Email = reader.GetString(2),
                PasswordHash = (byte[])reader["PasswordHash"],
                PasswordSalt = (byte[])reader["PasswordSalt"],
                IsEmailVerified = true
            };
        }



        public async Task<bool> DeletePendingUserAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM PendingUsers WHERE Email = @Email";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }
    }
}
