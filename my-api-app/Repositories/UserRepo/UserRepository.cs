using Microsoft.Data.SqlClient;
using my_api_app.Core.Exceptions.BusinessExceptions.ServerExceptions;
using my_api_app.Domain.Enums;
using my_api_app.Domain.Models;
using my_api_app.Infrastructure.Database;
using System.Data;
using System.Reflection;

namespace my_api_app.Repositories.UserRepo
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



        public async Task<Domain.Models.User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            const string sql = "SELECT UserID, Name, Email, PasswordHash, PasswordSalt FROM Users WHERE LOWER(Email)=LOWER(@Email) AND IsEmailVerified = 1 AND IsActive = 1;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync(cancellationToken);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new Domain.Models.User
            {
                UserID = reader.GetGuid(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = (byte[])reader["PasswordHash"],
                PasswordSalt = (byte[])reader["PasswordSalt"]
            };
        }



        public async Task<UserDetails?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            const string sql = "SELECT UserID, Name, Email, Gender, IsEmailVerified, IsActive, CreatedAt, UpdatedAt FROM Users WHERE UserID = @UserId;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UserId", userId);

            await con.OpenAsync(cancellationToken);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new UserDetails
            {
                UserId = reader.GetGuid(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Gender = Enum.Parse<Gender>(reader.GetString(3)),
                IsEmailVerified = reader.GetBoolean(4),
                IsActive = reader.GetBoolean(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.GetDateTime(7),
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



        public async Task<(IEnumerable<UserDetails> Items, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var users = new List<UserDetails>();
            int totalCount = 0;

            const string countQuery = @"SELECT COUNT(*) FROM Users WITH (NOLOCK);";

            const string dataQuery = @"SELECT UserID, Name, Email, Gender, IsEmailVerified, IsActive, CreatedAt, UpdatedAt FROM Users 
                WITH (NOLOCK) ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using SqlConnection con = _factory.CreateConnection();

            // COUNT QUERY
            using SqlCommand countCmd = new SqlCommand(countQuery, con);

            countCmd.CommandType = CommandType.Text;

            await con.OpenAsync(cancellationToken);

            var result = await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            totalCount = result == null ? 0 : Convert.ToInt32(result);


            // PAGED DATA QUERY
            using SqlCommand dataCmd = new SqlCommand(dataQuery, con);

            dataCmd.CommandType = CommandType.Text;
            dataCmd.Parameters.Add("@Offset", SqlDbType.Int).Value = (pageNumber - 1) * pageSize;
            dataCmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;

            using var reader = await dataCmd.ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SequentialAccess, cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var userDetails = new UserDetails
                {
                    UserId = reader.GetGuid(reader.GetOrdinal("UserID")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Gender = ParseGender(reader),
                    IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                };

                users.Add(userDetails);
            }

            return (users, totalCount);
        }



        private Gender? ParseGender(SqlDataReader reader)
        {
            var genderOrdinal = reader.GetOrdinal("Gender");

            if (reader.IsDBNull(genderOrdinal))
                return null;

            var value = reader.GetString(genderOrdinal);

            if (Enum.TryParse<Gender>(value, ignoreCase: true, out var genderEnum))
                return genderEnum;

            return null;
        }



        public async Task<UserDetails?> UpdateUserAsync(UserDetails user, CancellationToken cancellationToken)
        {
            const string sql = @"UPDATE Users SET Name = @Name, Gender = @Gender, UpdatedAt = @UpdatedAt WHERE UserID = @UserID;
                SELECT UserID, Name, Gender, Email, IsEmailVerified, IsActive, CreatedAt, UpdatedAt  FROM Users WITH (NOLOCK)  WHERE UserID = @UserID;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = user.UserId;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar, 200).Value = user.Name ?? (object)DBNull.Value;
            cmd.Parameters.Add("@Gender", SqlDbType.VarChar, 200).Value = user.Gender?.ToString() ?? (object)DBNull.Value;
            cmd.Parameters.Add("@UpdatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;

            await con.OpenAsync(cancellationToken);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new UserDetails
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Gender = ParseGender(reader),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }



        public async Task<UserDetails?> PatchUserAsync(Guid id, UserDetails request, CancellationToken cancellationToken)
        {
            var setClauses = new List<string>();
            var parameters = new List<SqlParameter>();

            if (request.Name is not null)
            {
                setClauses.Add("Name = @Name");
                parameters.Add(new SqlParameter("@Name", SqlDbType.VarChar, 200)
                {
                    Value = request.Name
                });
            }

            if (request.Gender is not null)
            {
                var genderString = request.Gender.ToString(); // Converts 1 → "Male", etc.

                setClauses.Add("Gender = @Gender");
                parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 50)
                {
                    Value = genderString
                });
            }

            // Nothing to update: skip SQL, return current record
            if (setClauses.Count == 0)
                return await GetUserByIdAsync(id, cancellationToken);

            setClauses.Add("UpdatedAt = @UpdatedAt");
            parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2)
            {
                Value = DateTime.UtcNow
            });

            parameters.Add(new SqlParameter("@UserID", SqlDbType.UniqueIdentifier)
            {
                Value = id
            });



            var sql = $@"UPDATE Users SET {string.Join(", ", setClauses)} WHERE UserID = @UserID;
                SELECT UserID, Name, Gender, Email, IsEmailVerified, IsActive, CreatedAt, UpdatedAt FROM Users WHERE UserID = @UserID;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddRange(parameters.ToArray());

            await con.OpenAsync(cancellationToken);

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new UserDetails
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Gender = ParseGender(reader),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }



        public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            const string sql = @"DELETE FROM Users WHERE UserID = @UserID;";

            using SqlConnection con = _factory.CreateConnection();
            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = userId;

            await con.OpenAsync(cancellationToken);

            var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            return affected > 0;
        }
    }
}
