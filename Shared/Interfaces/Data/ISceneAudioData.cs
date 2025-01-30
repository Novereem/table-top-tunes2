using Shared.Enums;
using Shared.Models;
using Shared.Models.Common;

namespace Shared.Interfaces.Data
{
    public interface ISceneAudioData
    {
        Task<DataResult<SceneAudioFile>> AddSceneAudioFileAsync(SceneAudioFile sceneAudioFile);
        Task<DataResult<bool>> RemoveSceneAudioFileAsync(SceneAudioFile sceneAudioFile);
        Task<DataResult<List<SceneAudioFile>>> GetSceneAudioFilesBySceneIdAsync(Guid sceneId);
    }
}