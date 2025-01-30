using MySqlConnector;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Data
{
    public class SceneAudioData : ISceneAudioData
    {
        public async Task<DataResult<SceneAudioFile>> AddSceneAudioFileAsync(SceneAudioFile sceneAudioFile)
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

            try
            {
                // Check if the record already exists
                var exists = await context.ExecuteScalarAsync<long>(checkExistenceQuery,
                    new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                    new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                    new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString()));

                if (exists > 0)
                {
                    return DataResult<SceneAudioFile>.AlreadyExists();
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
                    return DataResult<SceneAudioFile>.Success(new SceneAudioFile
                    {
                        SceneId = reader.GetGuid("SceneId"),
                        AudioFileId = reader.GetGuid("AudioFileId"),
                        AudioType = Enum.Parse<AudioType>(reader.GetString("AudioType"))
                    });
                }

                return DataResult<SceneAudioFile>.Error();
            }
            catch
            {
                return DataResult<SceneAudioFile>.Error();
            }
        }
        
        public async Task<DataResult<bool>> RemoveSceneAudioFileAsync(SceneAudioFile sceneAudioFile)
        {
            const string deleteQuery = @"
        DELETE FROM SceneAudioFile 
        WHERE SceneId = @SceneId AND AudioFileId = @AudioFileId AND AudioType = @AudioType;";

            using var context = new DatabaseContext();

            try
            {
                await context.OpenAsync();

                var rowsAffected = await context.ExecuteNonQueryAsync(deleteQuery,
                    new MySqlParameter("@SceneId", sceneAudioFile.SceneId),
                    new MySqlParameter("@AudioFileId", sceneAudioFile.AudioFileId),
                    new MySqlParameter("@AudioType", sceneAudioFile.AudioType.ToString())
                );

                if (rowsAffected > 0)
                {
                    return DataResult<bool>.Success(true);
                }

                return DataResult<bool>.NotFound();
            }
            catch
            {
                return DataResult<bool>.Error();
            }
        }
        
        public async Task<DataResult<List<SceneAudioFile>>> GetSceneAudioFilesBySceneIdAsync(Guid sceneId)
        {
            const string query = @"
        SELECT SceneId, AudioFileId, AudioType 
        FROM SceneAudioFile 
        WHERE SceneId = @SceneId;";

            using var context = new DatabaseContext();

            try
            {
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

                if (sceneAudioFiles.Count > 0)
                {
                    return DataResult<List<SceneAudioFile>>.Success(sceneAudioFiles);
                }

                return DataResult<List<SceneAudioFile>>.NotFound();
            }
            catch
            {
                return DataResult<List<SceneAudioFile>>.Error();
            }
        }
    }
}