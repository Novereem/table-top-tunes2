using System.Security.Claims;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;

namespace Shared.Interfaces.Services
{
    public interface ISceneAudioService
    {
        Task<HttpServiceResult<SceneAudioAssignResponseDTO>> AssignAudio(SceneAudioAssignDTO sceneAudioAssignDTO, ClaimsPrincipal user);
        Task<HttpServiceResult<bool>> RemoveAudio(SceneAudioRemoveDTO sceneAudioRemoveDTO, ClaimsPrincipal user);
        Task<HttpServiceResult<List<SceneAudioFile>>> GetSceneAudioFilesBySceneIdAsync(
            SceneAudioGetDTO sceneAudioGetDTO, ClaimsPrincipal user);
        Task<HttpServiceResult<bool>> RemoveAllAudioForSceneAsync(SceneAudioRemoveAllDTO sceneAudioRemoveAllDTO,
            ClaimsPrincipal user);
    }
}