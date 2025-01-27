using MySqlConnector;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Models;

namespace TTT2.Data
{
    public class SceneAudioData : ISceneAudioData
    {
        public async Task<SceneAudioFile?> AddSceneAudioFileAsync(SceneAudioFile sceneAudioFile)
        {
            const string checkExistenceQuery = @"
                SELECT COUNT(*) 
                FROM SceneAudioFile 
                WHERE SceneId = @SceneId AND AudioFileId = @AudioFileId AND AudioType = @AudioType;";

            const string insertQuery = @"
                INSERT INTO SceneAudioFile (SceneId, AudioFileId, AudioType) 
                VALUES (@SceneId, @AudioFileId, @AudioType);";

            const string selectQuery = @"
                SELECT SceneId, AudioFileId, AudioType 
                FROM SceneAudioFile 
                WHERE SceneId = @SceneId AND AudioFileId = @AudioFileId AND AudioType = @AudioType;";

            using var context = new DatabaseContext();
            await context.OpenAsync();

            // Check if the record already exists
            var exists = await context.ExecuteScalarAsync<long>(checkExistenceQuery,
                new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString()));

            if (exists > 0)
            {
                // Return null or handle the existence case as needed
                return null;
            }

            // Insert the record
            await context.ExecuteNonQueryAsync(insertQuery,
                new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString()));

            // Retrieve the inserted record
            await using var reader = await context.ExecuteQueryAsync(selectQuery,
                new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString()));

            if (await reader.ReadAsync())
            {
                return new SceneAudioFile
                {
                    SceneId = reader.GetGuid("SceneId"),
                    AudioFileId = reader.GetGuid("AudioFileId"),
                    AudioType = Enum.Parse<AudioType>(reader.GetString("AudioType"))
                };
            }

            return null;
        }
        
        public async Task<bool> RemoveSceneAudioFileAsync(SceneAudioFile sceneAudioFile)
        {
            const string deleteQuery = @"
        DELETE FROM SceneAudioFile 
        WHERE SceneId = @SceneId AND AudioFileId = @AudioFileId AND AudioType = @AudioType;";

            using var context = new DatabaseContext();

            await context.OpenAsync();

            var rowsAffected = await context.ExecuteNonQueryAsync(deleteQuery,
                new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString())
            );

            return rowsAffected > 0;
        }
        
        public async Task<List<SceneAudioFile>> GetSceneAudioFilesBySceneIdAsync(Guid sceneId)
        {
            const string query = @"
        SELECT SceneId, AudioFileId, AudioType 
        FROM SceneAudioFile 
        WHERE SceneId = @SceneId;";

            using var context = new DatabaseContext();

            await context.OpenAsync();

            await using var reader = await context.ExecuteQueryAsync(query, 
                new MySqlParameter("@SceneId", sceneId)
            );

            var sceneAudioFiles = new List<SceneAudioFile>();
            while (await reader.ReadAsync())
            {
                sceneAudioFiles.Add(new SceneAudioFile
                {
                    SceneId = reader.GetGuid("SceneId"),
                    AudioFileId = reader.GetGuid("AudioFileId"),
                    AudioType = Enum.Parse<AudioType>(reader.GetString("AudioType"))
                });
            }

            return sceneAudioFiles;
        }
    }
}