using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.Extensions;

namespace TTT2.Services
{
    public class SceneAudioService(
        IUserClaimsService userClaimsService, 
        IAudioService audioService, 
        ISceneAudioServiceHelper helper, 
        IAuthenticationService authenticationService,
        ISceneValidationService sceneValidationService)
        : ISceneAudioService
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
                    return HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(validateOwnership.ToFailureResult<SceneAudioAssignResponseDTO>());
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
                    await sceneValidationService.ValidateSceneWithUserAsync(sceneAudioGetDTO.SceneId, userIdResult.Data);
                if (validScene.IsFailure)
                {
                    return HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(validScene.ToFailureResult<List<SceneAudioFile>>());
                }
                var sceneAudios = await helper.GetSceneAudioFilesAsync(sceneAudioGetDTO);
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
                    return HttpServiceResult<bool>.FromServiceResult(validOwnershipResult.ToFailureResult<bool>());
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
        
        public async Task<HttpServiceResult<bool>> RemoveAllAudioForSceneAsync(SceneAudioRemoveAllDTO sceneAudioRemoveAllDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<bool>.FromServiceResult(userIdResult.ToFailureResult<bool>());
            }
            
            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<bool>.FromServiceResult(userResult.ToFailureResult<bool>());
            }
            
            try
            {
                // Validate scene ownership
                var validScene = await sceneValidationService.ValidateSceneWithUserAsync(sceneAudioRemoveAllDTO.SceneId, userResult.Data.Id);
                if (validScene.IsFailure)
                {
                    return HttpServiceResult<bool>.FromServiceResult(validScene.ToFailureResult<bool>());
                }

                var deleteAllResult = await helper.RemoveAllSceneAudioFilesAsync(sceneAudioRemoveAllDTO);
                if (deleteAllResult.IsFailure)
                {
                    return HttpServiceResult<bool>.FromServiceResult(deleteAllResult.ToFailureResult<bool>());
                }

                return HttpServiceResult<bool>.FromServiceResult(
                    ServiceResult<bool>.SuccessResult(true, MessageKey.Success_AllSceneAudiosRemoval));
            }
            catch
            {
                return HttpServiceResult<bool>.FromServiceResult(
                    ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorService));
            }
        }

        private async Task<ServiceResult<bool>> ValidateSceneAudioOwnership(Guid sceneId, Guid audioFileId, Guid userId)
        {
            var validScene =
                await sceneValidationService.ValidateSceneWithUserAsync(sceneId, userId);
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