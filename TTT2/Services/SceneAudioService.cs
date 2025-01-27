using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.Extensions;

namespace TTT2.Services
{
    public class SceneAudioService(IUserClaimsService userClaimsService, ISceneService sceneService, IAudioService audioService, ISceneAudioServiceHelper helper) : ISceneAudioService
    {
        public async Task<HttpServiceResult<SceneAudioAssignResponseDTO>> AssignAudio(SceneAudioAssignDTO sceneAudioAssignDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<SceneAudioAssignResponseDTO>());
            }

            try
            {
                var validScene =
                    await sceneService.ValidateSceneWithUserAsync(sceneAudioAssignDTO.SceneId, userIdResult.Data);
                var validAudio =
                    await audioService.ValidateAudioFileWithUserAsync(sceneAudioAssignDTO.AudioFileId,
                        userIdResult.Data);
                if (!validScene.Data || !validAudio.Data)
                {
                    return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_Unauthorized));
                }

                var assignedSceneAudio = await helper.AddSceneAudioFileAsync(sceneAudioAssignDTO);
                if (assignedSceneAudio.IsFailure)
                {
                    return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(assignedSceneAudio.ToFailureResult<SceneAudioAssignResponseDTO>());
                }
                return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(
                    ServiceResult<SceneAudioAssignResponseDTO>.SuccessResult(assignedSceneAudio.Data, MessageKey.Success_SceneAudioAssignment)
                );
            }
            catch
            {
                return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_InternalServerError));
            }
        }
    }
}