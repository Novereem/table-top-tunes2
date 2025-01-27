using Shared.Enums;
using Shared.Models;

namespace Shared.Interfaces.Data
{
    public interface ISceneAudioData
    {
        Task<SceneAudioFile?> AddSceneAudioFileAsync(SceneAudioFile sceneAudioFile);
        Task<bool> RemoveSceneAudioFileAsync(SceneAudioFile sceneAudioFile);
        Task<List<SceneAudioFile>> GetSceneAudioFilesBySceneIdAsync(Guid sceneId);
    }
}