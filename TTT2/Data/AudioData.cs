using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;

namespace TTT2.Data;

public class AudioData : IAudioData
{
    public async Task<AudioFile?> SaveAudioFileAsync(AudioFile audioFile)
    {
        const string insertQuery = "INSERT INTO AudioFiles (Id, Name, FilePath ,UserId) VALUES (@Id, @Name, @FilePath ,@UserId);";
        const string selectQuery = "SELECT Id, Name, UserId, FilePath, CreatedAt FROM AudioFiles WHERE Id = @Id;";

        using var context = new DatabaseContext();

        await context.OpenAsync();

        // Insert the scene
        await context.ExecuteNonQueryAsync(insertQuery,
            new MySqlParameter("@Id", audioFile.Id),
            new MySqlParameter("@Name", audioFile.Name),
            new MySqlParameter("@FilePath", audioFile.FilePath),
            new MySqlParameter("@UserId", audioFile.UserId));

        // Retrieve the inserted scene
        await using var reader = await context.ExecuteQueryAsync(selectQuery, new MySqlParameter("@Id", audioFile.Id));
        if (await reader.ReadAsync())
        {
            return new AudioFile
            {
                Id = reader.GetGuid("Id"),
                Name = reader.GetString("Name"),
                FilePath = reader.GetString("FilePath"),
                UserId = reader.GetGuid("UserId"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }

        return null;
    }
    
    public async Task<bool> AudioFileBelongsToUserAsync(Guid audioFileId, Guid userId)
    {
        const string query = "SELECT COUNT(*) FROM AudioFiles WHERE Id = @AudioFileId AND UserId = @UserId;";
        using var context = new DatabaseContext();

        await context.OpenAsync();
        var count = await context.ExecuteScalarAsync<int>(query, 
            new MySqlParameter("@AudioFileId", audioFileId),
            new MySqlParameter("@UserId", userId)
        );

        return count > 0;
    }
}