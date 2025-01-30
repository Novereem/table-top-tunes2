using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Data
{
    public class SceneData : ISceneData
    {
        public async Task<DataResult<Scene>> CreateSceneAsync(Scene scene)
        {
            const string insertQuery = "INSERT INTO Scenes (Id, Name, UserId) VALUES (@Id, @Name, @UserId);";
            const string selectQuery = "SELECT Id, Name, UserId, CreatedAt FROM Scenes WHERE Id = @Id;";

            using var context = new DatabaseContext();
            await context.OpenAsync();

            try
            {
                await context.ExecuteNonQueryAsync(insertQuery,
                    new MySqlParameter("@Id", scene.Id),
                    new MySqlParameter("@Name", scene.Name),
                    new MySqlParameter("@UserId", scene.UserId));

                await using var reader = await context.ExecuteQueryAsync(selectQuery, new MySqlParameter("@Id", scene.Id));

                if (await reader.ReadAsync())
                {
                    return DataResult<Scene>.Success(new Scene
                    {
                        Id = reader.GetGuid("Id"),
                        Name = reader.GetString("Name"),
                        UserId = reader.GetGuid("UserId"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }

                return DataResult<Scene>.Error();
            }
            catch
            {
                return DataResult<Scene>.Error();
            }
        }

        public async Task<DataResult<List<Scene>>> GetScenesByUserIdAsync(Guid userId)
        {
            const string query = "SELECT Id, Name, UserId, CreatedAt FROM Scenes WHERE UserId = @UserId;";
            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();
                await using var reader = await context.ExecuteQueryAsync(query, new MySqlParameter("@UserId", userId));

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

                return scenes.Count > 0 
                    ? DataResult<List<Scene>>.Success(scenes) 
                    : DataResult<List<Scene>>.NotFound();
            }
            catch
            {
                return DataResult<List<Scene>>.Error();
            }
        }
        
        public async Task<DataResult<bool>> SceneBelongsToUserAsync(Guid sceneId, Guid userId)
        {
            const string query = "SELECT COUNT(*) FROM Scenes WHERE Id = @SceneId AND UserId = @UserId;";
            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();
                var count = await context.ExecuteScalarAsync<long>(query, 
                    new MySqlParameter("@SceneId", sceneId),
                    new MySqlParameter("@UserId", userId)
                );

                return count > 0 ? DataResult<bool>.Success(true) : DataResult<bool>.NotFound();
            }
            catch
            {
                return DataResult<bool>.Error();
            }
        }
    }
}
