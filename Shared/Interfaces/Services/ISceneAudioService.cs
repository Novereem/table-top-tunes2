using System.Security.Claims;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;

namespace Shared.Interfaces.Services
{
    public interface ISceneAudioService
    {
        Task<HttpServiceResult<SceneAudioAssignResponseDTO>> AssignAudio(SceneAudioAssignDTO sceneAudioAssignDTO, ClaimsPrincipal user);
    }
}