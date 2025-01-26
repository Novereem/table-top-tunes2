using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;

namespace TTT2.Data
{
    public class SceneData : ISceneData
    {
        public async Task<Scene?> CreateSceneAsync(Scene scene)
        {
            const string insertQuery = "INSERT INTO Scenes (Id, Name, UserId) VALUES (@Id, @Name, @UserId);";
            const string selectQuery = "SELECT Id, Name, UserId, CreatedAt FROM Scenes WHERE Id = @Id;";

            using var context = new DatabaseContext();

            await context.OpenAsync();

            // Insert the scene
            await context.ExecuteNonQueryAsync(insertQuery,
                new MySqlParameter("@Id", scene.Id),
                new MySqlParameter("@Name", scene.Name),
                new MySqlParameter("@UserId", scene.UserId));

            // Retrieve the inserted scene
            using var reader = await context.ExecuteQueryAsync(selectQuery, new MySqlParameter("@Id", scene.Id));
            if (await reader.ReadAsync())
            {
                return new Scene
                {
                    Id = reader.GetGuid("Id"),
                    Name = reader.GetString("Name"),
                    UserId = reader.GetGuid("UserId"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

        public async Task<List<Scene>> GetScenesByUserIdAsync(Guid userId)
        {
            const string query = "SELECT Id, Name, UserId, CreatedAt FROM Scenes WHERE UserId = @UserId;";
            using var context = new DatabaseContext();

            await context.OpenAsync();
            using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@UserId", userId));

            var scenes = new List<Scene>();
            while (await reader.ReadAsync())
            {
                scenes.Add(new Scene
                {
                    Id = reader.GetGuid("Id"),
                    Name = reader.GetString("Name"),
                    UserId = reader.GetGuid("UserId"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return scenes;
        }
    }
}
