using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Data;

public class AudioData : IAudioData
{
    public async Task<DataResult<AudioFile>> SaveAudioFileAsync(AudioFile audioFile)
    {
        const string insertQuery = "INSERT INTO AudioFiles (Id, Name ,UserId) VALUES (@Id, @Name ,@UserId);";
        const string selectQuery = "SELECT Id, Name, UserId, CreatedAt FROM AudioFiles WHERE Id = @Id;";

        using var context = new DatabaseContext();
        await context.OpenAsync();

        try
        {
            await context.ExecuteNonQueryAsync(insertQuery,
                new MySqlParameter("@Id", audioFile.Id),
                new MySqlParameter("@Name", audioFile.Name),
                new MySqlParameter("@UserId", audioFile.UserId));

            await using var reader = await context.ExecuteQueryAsync(selectQuery, new MySqlParameter("@Id", audioFile.Id));

            if (await reader.ReadAsync())
            {
                return DataResult<AudioFile>.Success(new AudioFile
                {
                    Id = reader.GetGuid("Id"),
                    Name = reader.GetString("Name"),
                    UserId = reader.GetGuid("UserId"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return DataResult<AudioFile>.Error();
        }
        catch
        {
            return DataResult<AudioFile>.Error();
        }
    }
    
    public async Task<DataResult<bool>> RemoveAudioFileAsync(Guid audioFileId, Guid userId)
    {
        const string deleteQuery = "DELETE FROM AudioFiles WHERE Id = @AudioFileId AND UserId = @UserId;";
        using var context = new DatabaseContext();
        
        try
        {
            await context.OpenAsync();
            var rowsAffected = await context.ExecuteNonQueryAsync(deleteQuery,
                new MySqlParameter("@AudioFileId", audioFileId),
                new MySqlParameter("@UserId", userId)
            );

            return rowsAffected > 0 ? DataResult<bool>.Success(true) : DataResult<bool>.NotFound();
        }
        catch
        {
            return DataResult<bool>.Error();
        }
    }
    
    public async Task<DataResult<bool>> AudioFileBelongsToUserAsync(Guid audioFileId, Guid userId)
    {
        const string query = "SELECT COUNT(*) FROM AudioFiles WHERE Id = @AudioFileId AND UserId = @UserId;";
        using var context = new DatabaseContext();

        try
        {
            await context.OpenAsync();
            var count = await context.ExecuteScalarAsync<long>(query, 
                new MySqlParameter("@AudioFileId", audioFileId),
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