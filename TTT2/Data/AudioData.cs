using MySqlConnector;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Data;

public class AudioData : IAudioData
{
    public async Task<DataResult<AudioFile>> SaveAudioFileAsync(AudioFile audioFile)
    {
        const string insertQuery = "INSERT INTO AudioFiles (Id, Name ,UserId, FileSize) VALUES (@Id, @Name ,@UserId, @FileSize);";
        const string selectQuery = "SELECT Id, Name, UserId, FileSize ,CreatedAt FROM AudioFiles WHERE Id = @Id;";

        using var context = new DatabaseContext();
        await context.OpenAsync();

        try
        {
            await context.ExecuteNonQueryAsync(insertQuery,
                new MySqlParameter("@Id", audioFile.Id),
                new MySqlParameter("@Name", audioFile.Name),
                new MySqlParameter("@UserId", audioFile.UserId),
                new MySqlParameter("@FileSize", audioFile.FileSize));

            await using var reader = await context.ExecuteQueryAsync(selectQuery, new MySqlParameter("@Id", audioFile.Id));

            if (await reader.ReadAsync())
            {
                return DataResult<AudioFile>.Success(new AudioFile
                {
                    Id = reader.GetGuid("Id"),
                    Name = reader.GetString("Name"),
                    UserId = reader.GetGuid("UserId"),
                    FileSize = reader.GetInt64("FileSize"),
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
    
    public async Task<DataResult<long>> RemoveAudioFileAsync(Guid audioFileId, Guid userId)
    {
        const string selectQuery = "SELECT FileSize FROM AudioFiles WHERE Id = @Id AND UserId = @UserId;";
        const string deleteQuery = "DELETE FROM AudioFiles WHERE Id = @Id AND UserId = @UserId;";

        using var context = new DatabaseContext();
        await context.OpenAsync();

        try
        {
            var fileSize = await context.ExecuteScalarAsync<long?>(selectQuery,
                new MySqlParameter("@Id", audioFileId),
                new MySqlParameter("@UserId", userId)
            );

            if (fileSize == null)
            {
                return DataResult<long>.NotFound();
            }

            var rowsAffected = await context.ExecuteNonQueryAsync(deleteQuery,
                new MySqlParameter("@Id", audioFileId),
                new MySqlParameter("@UserId", userId)
            );

            if (rowsAffected > 0)
            {
                return DataResult<long>.Success(fileSize.Value);
            }

            return DataResult<long>.Error();
        }
        catch
        {
            return DataResult<long>.Error();
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