using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;

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
                var validateOwnership = await ValidateSceneAudioOwnership(sceneAudioAssignDTO.SceneId, sceneAudioAssignDTO.AudioFileId, userIdResult.Data);
                if (validateOwnership.IsFailure)
                {
                    return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<SceneAudioAssignResponseDTO>());
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

        public async Task<HttpServiceResult<bool>> RemoveAudio(SceneAudioRemoveDTO sceneAudioRemoveDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<bool>.FromServiceResult(userIdResult.ToFailureResult<bool>());
            }

            try
            {
                var validOwnershipResult = await ValidateSceneAudioOwnership(sceneAudioRemoveDTO.SceneId,
                    sceneAudioRemoveDTO.AudioFileId,
                    userIdResult.Data);
                if (validOwnershipResult.IsFailure)
                {
                    return HttpServiceResult<bool>.FromServiceResult(userIdResult.ToFailureResult<bool>());
                }

                var removeSceneAudio = await helper.RemoveSceneAudioFileAsync(sceneAudioRemoveDTO);
                if (removeSceneAudio.IsFailure)
                {
                    return HttpServiceResult<bool>.FromServiceResult(removeSceneAudio.ToFailureResult<bool>());
                }

                return HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.SuccessResult(true, MessageKey.Success_SceneAudioRemoval));
            }
            catch
            {
                return HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.Failure(MessageKey.Error_InternalServerError));
            }
        }

        public async Task<HttpServiceResult<List<SceneAudioFile>>> GetSceneAudioFilesBySceneIdAsync(
            SceneAudioGetDTO sceneAudioGetDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(userIdResult.ToFailureResult<List<SceneAudioFile>>());
            }

            try
            {
                var validScene =
                    await sceneService.ValidateSceneWithUserAsync(sceneAudioGetDTO.SceneId, userIdResult.Data);
                if (validScene.IsFailure)
                {
                    return HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(validScene.ToFailureResult<List<SceneAudioFile>>());
                }
                var sceneAudios = await helper.GetSceneAudioFilesAsync(sceneAudioGetDTO.SceneId);
                if (sceneAudios.IsFailure)
                {
                    return HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(sceneAudios.ToFailureResult<List<SceneAudioFile>>());
                }
                return HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(ServiceResult<List<SceneAudioFile>>.SuccessResult(sceneAudios.Data, MessageKey.Success_SceneAudioFilesRetrieval));
            }
            catch
            {
                return HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(ServiceResult<List<SceneAudioFile>>.Failure(MessageKey.Error_InternalServerError));
            }
        }

        private async Task<ServiceResult<bool>> ValidateSceneAudioOwnership(Guid sceneId, Guid audioFileId, Guid userId)
        {
            var validScene =
                await sceneService.ValidateSceneWithUserAsync(sceneId, userId);
            if (validScene.IsFailure)
            {
                return validScene.ToFailureResult<bool>();
            }
            var validAudio =
                await audioService.ValidateAudioFileWithUserAsync(audioFileId, userId);
            if (validAudio.IsFailure)
            {
                return validAudio.ToFailureResult<bool>();
            }
            return ServiceResult<bool>.SuccessResult(true);
        }
    }
}