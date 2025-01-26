using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;

namespace TTT2.Data
{
    public class AuthenticationData : IAuthenticationData
    {
        public async Task RegisterUserAsync(User user)
        {
            const string query = "INSERT INTO Users (Id, Username, Email, PasswordHash) VALUES (@Id, @Username, @Email, @PasswordHash);";
            using var context = new DatabaseContext();

            await context.OpenAsync();
            await context.ExecuteNonQueryAsync(query,
                new MySqlParameter("@Id", user.Id),
                new MySqlParameter("@Username", user.Username),
                new MySqlParameter("@Email", user.Email),
                new MySqlParameter("@PasswordHash", user.PasswordHash));
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            const string query = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Username = @Username;";
            using var context = new DatabaseContext();

            await context.OpenAsync();
            using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@Username", username));

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string query = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Email = @Email;";
            using var context = new DatabaseContext();

            await context.OpenAsync();
            using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@Email", email));

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            const string query = "SELECT Id, Username, Email, PasswordHash, CreatedAt FROM Users WHERE Id = @Id;";
            using var context = new DatabaseContext();

            await context.OpenAsync();
            using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@Id", userId.ToString()));

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetGuid("Id"),
                    Username = reader.GetString("Username"),
                    Email = reader.GetString("Email"),
                    PasswordHash = reader.GetString("PasswordHash"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }
    }
}
