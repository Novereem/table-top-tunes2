using System.Collections.Concurrent;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Tests.FakeData;

public class FakeAudioData : IAudioData
{
    // In-memory store keyed by AudioFile.Id.
    private readonly ConcurrentDictionary<Guid, AudioFile> _audioFiles = new();

    public Task<DataResult<AudioFile>> SaveAudioFileAsync(AudioFile audioFile)
    {
        audioFile.CreatedAt = DateTime.UtcNow;
        _audioFiles[audioFile.Id] = audioFile;
        return Task.FromResult(DataResult<AudioFile>.Success(audioFile));
    }

    public Task<DataResult<long>> RemoveAudioFileAsync(Guid audioFileId, Guid userId)
    {
        if (_audioFiles.TryGetValue(audioFileId, out var audioFile) && audioFile.UserId == userId)
        {
            long fileSize = audioFile.FileSize;
            _audioFiles.TryRemove(audioFileId, out _);
            return Task.FromResult(DataResult<long>.Success(fileSize));
        }
        return Task.FromResult(DataResult<long>.NotFound());
    }

    public Task<DataResult<bool>> AudioFileBelongsToUserAsync(Guid audioFileId, Guid userId)
    {
        if (_audioFiles.TryGetValue(audioFileId, out var audioFile) && audioFile.UserId == userId)
        {
            return Task.FromResult(DataResult<bool>.Success(true));
        }
        return Task.FromResult(DataResult<bool>.NotFound());
    }

    // Optional helper to clear the store between tests.
    public void Clear() => _audioFiles.Clear();
}