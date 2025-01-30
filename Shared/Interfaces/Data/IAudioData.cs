using Shared.Models;
using Shared.Models.Common;

namespace Shared.Interfaces.Data;

public interface IAudioData
{
    Task<DataResult<AudioFile>> SaveAudioFileAsync(AudioFile audioFile);
    Task<DataResult<bool>> RemoveAudioFileAsync(Guid audioFileId, Guid userId);
    Task<DataResult<bool>> AudioFileBelongsToUserAsync(Guid audioFileId, Guid userId);
}