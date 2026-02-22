using Microsoft.Data.SqlClient;
using my_api_app.Data;
using my_api_app.Enums;
using my_api_app.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Models.Auth;
using my_api_app.Repositories.Interfaces;

namespace my_api_app.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _factory;

        public UserRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }



        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE LOWER(Email)=LOWER(@Email);";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync(cancellationToken);
            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));

            return count > 0;
        }



        public async Task<CreatedUserResult> CreateUserAsync(string name, string email, Gender? gender, byte[] hash, byte[] salt, CancellationToken cancellationToken)
        {
            const string sql = @"
                INSERT INTO Users (Name, Email, Gender, PasswordHash, PasswordSalt, IsEmailVerified)
                OUTPUT INSERTED.UserId, INSERTED.CreatedAt
                VALUES (@Name, @Email, @Gender, @Hash, @Salt, 1);";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Gender", gender.HasValue ? gender.Value.ToString() : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Hash", hash);
            cmd.Parameters.AddWithValue("@Salt", salt);

            await con.OpenAsync(cancellationToken);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                throw new InternalServerException(); // INSERT succeeded but returned no row

            return new CreatedUserResult
            {
                UserID = reader.GetGuid(0),
                CreatedAt = reader.GetDateTime(1)
            };
        }



        public async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = "SELECT UserID, Name, Email, PasswordHash, PasswordSalt FROM Users WHERE LOWER(Email)=LOWER(@Email) AND IsEmailVerified = 1 AND IsActive = 1;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync(cancellationToken);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new User
            {
                UserID = reader.GetGuid(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = (byte[])reader["PasswordHash"],
                PasswordSalt = (byte[])reader["PasswordSalt"]
            };
        }



        public async Task<bool> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE Users SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt WHERE Email = @Email AND IsActive = 1;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync(cancellationToken);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);

            return rows == 1;
        }
    }
}
