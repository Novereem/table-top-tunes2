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
            const string insertQuery = @"
        INSERT INTO SceneAudioFile (SceneId, AudioFileId, AudioType) 
        VALUES (@SceneId, @AudioFileId, @AudioType);";
            const string selectQuery = @"
        SELECT SceneId, AudioFileId, AudioType 
        FROM SceneAudioFile 
        WHERE SceneId = @SceneId AND AudioFileId = @AudioFileId AND AudioType = @AudioType;";

            using var context = new DatabaseContext();

            await context.OpenAsync();

            // Insert the SceneAudioFile
            await context.ExecuteNonQueryAsync(insertQuery,
                new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString()));

            // Retrieve the inserted SceneAudioFile
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
    }
}