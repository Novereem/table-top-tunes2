using Shared.Models;

namespace Shared.Interfaces.Data;

public interface IAudioData
{
    Task<AudioFile?> SaveAudioFileAsync(AudioFile audioFile);
    Task<bool> AudioFileBelongsToUserAsync(Guid audioFileId, Guid userId);
}