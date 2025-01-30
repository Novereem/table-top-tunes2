using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;

namespace Shared.Interfaces.Services.Helpers
{
    public interface ISceneAudioServiceHelper
    {
        Task<ServiceResult<SceneAudioAssignResponseDTO>> AddSceneAudioFileAsync(SceneAudioAssignDTO sceneAudioAssignDTO);
        Task<ServiceResult<bool>> RemoveSceneAudioFileAsync(SceneAudioRemoveDTO sceneAudioRemoveDTO);
        Task<ServiceResult<List<SceneAudioFile>>> GetSceneAudioFilesAsync(SceneAudioGetDTO sceneAudioGetDTO);
    }
}