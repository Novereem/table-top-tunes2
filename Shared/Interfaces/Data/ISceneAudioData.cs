using Shared.Models;

namespace Shared.Interfaces.Data
{
    public interface ISceneAudioData
    {
        Task<SceneAudioFile?> AddSceneAudioFileAsync(SceneAudioFile sceneAudioFile);
    }
}