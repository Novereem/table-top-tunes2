using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Data
{
    public class AuthenticationData : IAuthenticationData
    {
        public async Task<DataResult<User>> RegisterUserAsync(User user)
        {
            const string insertQuery = "INSERT INTO Users (Id, Username, Email, PasswordHash) VALUES (@Id, @Username, @Email, @PasswordHash);";
            using var context = new DatabaseContext();
            
            try
            {
                await context.OpenAsync();
                var rowsAffected = await context.ExecuteNonQueryAsync(insertQuery,
                    new MySqlParameter("@Id", user.Id),
                    new MySqlParameter("@Username", user.Username),
                    new MySqlParameter("@Email", user.Email),
                    new MySqlParameter("@PasswordHash", user.PasswordHash));

                return rowsAffected > 0 ? DataResult<User>.Success(user) : DataResult<User>.Error();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("ERROR MESSAGE ^^^^^^^^^^");
                return DataResult<User>.Error();
            }
        }

        public async Task<DataResult<User>> GetUserByUsernameAsync(string username)
        {
            const string query = "SELECT Id, Username, Email, PasswordHash, UsedStorageBytes, MaxStorageBytes ,CreatedAt FROM Users WHERE Username = @Username;";
            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();
                await using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@Username", username));

                if (await reader.ReadAsync())
                {
                    return DataResult<User>.Success(new User
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Email = reader.GetString("Email"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        UsedStorageBytes = reader.GetInt64("UsedStorageBytes"),
                        MaxStorageBytes = reader.GetInt64("MaxStorageBytes"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }

                return DataResult<User>.NotFound();
            }
            catch
            {
                return DataResult<User>.Error();
            }
        }

        public async Task<DataResult<User>> GetUserByEmailAsync(string email)
        {
            const string query = "SELECT Id, Username, Email, PasswordHash, UsedStorageBytes, MaxStorageBytes ,CreatedAt FROM Users WHERE Email = @Email;";
            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();
                await using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@Email", email));

                if (await reader.ReadAsync())
                {
                    return DataResult<User>.Success(new User
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Email = reader.GetString("Email"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        UsedStorageBytes = reader.GetInt64("UsedStorageBytes"),
                        MaxStorageBytes = reader.GetInt64("MaxStorageBytes"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }

                return DataResult<User>.NotFound();
            }
            catch
            {
                return DataResult<User>.Error();
            }
        }

        public async Task<DataResult<User>> GetUserByIdAsync(Guid userId)
        {
            const string query = "SELECT Id, Username, Email, PasswordHash, UsedStorageBytes, MaxStorageBytes ,CreatedAt FROM Users WHERE Id = @Id;";
            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();
                await using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@Id", userId));

                if (await reader.ReadAsync())
                {
                    return DataResult<User>.Success(new User
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Email = reader.GetString("Email"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        UsedStorageBytes = reader.GetInt64("UsedStorageBytes"),
                        MaxStorageBytes = reader.GetInt64("MaxStorageBytes"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }

                return DataResult<User>.NotFound();
            }
            catch
            {
                return DataResult<User>.Error();
            }
        }
        
        public async Task<DataResult<User>> UpdateUserAsync(User user)
        {
            const string updateQuery = @"
                UPDATE Users
                SET 
                    Username = @Username, 
                    Email = @Email, 
                    PasswordHash = @PasswordHash,
                    UsedStorageBytes = @UsedStorageBytes,
                    MaxStorageBytes = @MaxStorageBytes
                WHERE Id = @Id;
            ";

            const string selectQuery = @"
                SELECT Id, Username, Email, PasswordHash, UsedStorageBytes, MaxStorageBytes, CreatedAt
                FROM Users
                WHERE Id = @Id;
            ";
            
            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();
        
                var rowsAffected = await context.ExecuteNonQueryAsync(updateQuery,
                    new MySqlParameter("@Id", user.Id),
                    new MySqlParameter("@Username", user.Username),
                    new MySqlParameter("@Email", user.Email),
                    new MySqlParameter("@PasswordHash", user.PasswordHash),
                    new MySqlParameter("@UsedStorageBytes", user.UsedStorageBytes),
                    new MySqlParameter("@MaxStorageBytes", user.MaxStorageBytes)
                );

                
                if (rowsAffected <= 0)
                {
                    return DataResult<User>.NotFound(); 
                }
                
                await using var reader = await context.ExecuteQueryAsync(selectQuery, new MySqlParameter("@Id", user.Id));
                if (await reader.ReadAsync())
                {
                    var updatedUser = new User
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Email = reader.GetString("Email"),
                        PasswordHash = reader.GetString("PasswordHash"),
                        UsedStorageBytes = reader.GetInt64("UsedStorageBytes"),
                        MaxStorageBytes = reader.GetInt64("MaxStorageBytes"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    };

                    return DataResult<User>.Success(updatedUser);
                }
                return DataResult<User>.NotFound();
            }
            catch
            {
                return DataResult<User>.Error();
            }
        }
    }
}
