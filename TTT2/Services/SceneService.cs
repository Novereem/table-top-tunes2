using Shared.Enums;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using System.Security.Claims;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers.Shared;
using IAuthenticationService = Shared.Interfaces.Services.IAuthenticationService;
using Shared.Models.Extensions;

namespace TTT2.Services
{
    public class SceneService(
        ISceneServiceHelper helper,
        IUserClaimsService userClaimsService,
        IAuthenticationService authenticationService,
        ISceneAudioService sceneAudioService,
        ISceneValidationService sceneValidationService)
        : ISceneService
    {
        public async Task<HttpServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<SceneCreateResponseDTO>());
            }

            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(userResult.ToFailureResult<SceneCreateResponseDTO>());
            }

            try
            {
                var validationResult = helper.ValidateSceneCreate(sceneDTO);
                if (validationResult.IsFailure)
                {
                    return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<SceneCreateResponseDTO>());
                }
                
                var createdSceneResult = await helper.CreateSceneAsync(sceneDTO, userResult.Data);
                if (createdSceneResult.IsFailure)
                {
                    return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(createdSceneResult.ToFailureResult<SceneCreateResponseDTO>());
                }
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(
                    ServiceResult<SceneCreateResponseDTO>.SuccessResult(createdSceneResult.Data, MessageKey.Success_SceneCreation)
                );
            }
            catch
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerError));
            }
        }

        public async Task<HttpServiceResult<List<SceneListItemDTO>>> GetScenesListByUserIdAsync(ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(userIdResult.ToFailureResult<List<SceneListItemDTO>>());
            }
            
            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(userResult.ToFailureResult<List<SceneListItemDTO>>());
            }
            
            try
            {
                var scenesResult = await helper.RetrieveScenesByUserIdAsync(userResult.Data);
                var sceneListItems = scenesResult.Data!.Select(scene => scene.ToListItemDTO()).ToList();
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(
                     ServiceResult<List<SceneListItemDTO>>.SuccessResult(sceneListItems, MessageKey.Success_DataRetrieved)
                 );
            }
            catch
            {
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(ServiceResult<List<SceneListItemDTO>>.Failure(MessageKey.Error_InternalServerError));
            }
        }

        public async Task<HttpServiceResult<SceneGetResponseDTO>> GetSceneByIdAsync(SceneGetDTO sceneGetDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<SceneGetResponseDTO>());
            }
            
            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(userResult.ToFailureResult<SceneGetResponseDTO>());
            }
            
            var validScene = await sceneValidationService.ValidateSceneWithUserAsync(sceneGetDTO.SceneId, userResult.Data.Id);
            if (validScene.IsFailure)
            {
                return HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(validScene.ToFailureResult<SceneGetResponseDTO>());
            }
            
            try
            {
                var sceneResult = await helper.RetrieveSceneBySceneIdAsync(sceneGetDTO);
                if (validScene.IsFailure)
                {
                    return HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(sceneResult.ToFailureResult<SceneGetResponseDTO>());
                }
                return HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(ServiceResult<SceneGetResponseDTO>.SuccessResult(sceneResult.Data, MessageKey.Success_DataRetrieved));
            }
            catch
            {
                return HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(ServiceResult<SceneGetResponseDTO>.Failure(MessageKey.Error_InternalServerError));
            }
        }
        
        public async Task<HttpServiceResult<SceneUpdateResponseDTO>> UpdateSceneAsync(SceneUpdateDTO sceneUpdateDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                    userIdResult.ToFailureResult<SceneUpdateResponseDTO>());
            }

            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                    userResult.ToFailureResult<SceneUpdateResponseDTO>());
            }
            
            var validScene = await sceneValidationService.ValidateSceneWithUserAsync(sceneUpdateDTO.SceneId, userResult.Data.Id);
            if (validScene.IsFailure)
            {
                return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                    validScene.ToFailureResult<SceneUpdateResponseDTO>());
            }
            
            try
            {
                var validationResult = helper.ValidateSceneUpdate(sceneUpdateDTO);
                if (validationResult.IsFailure)
                {
                    return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                        validationResult.ToFailureResult<SceneUpdateResponseDTO>());
                }
                
                var updateResult = await helper.UpdateSceneAsync(sceneUpdateDTO, userResult.Data);
                if (updateResult.IsFailure)
                {
                    return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                        updateResult.ToFailureResult<SceneUpdateResponseDTO>());
                }

                return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                    ServiceResult<SceneUpdateResponseDTO>.SuccessResult(updateResult.Data, MessageKey.Success_SceneUpdate)
                );
            }
            catch
            {
                return HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(
                    ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_InternalServerError));
            }
        }
        
        public async Task<HttpServiceResult<bool>> DeleteSceneAsync(SceneRemoveDTO sceneRemoveDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<bool>.FromServiceResult(
                    userIdResult.ToFailureResult<bool>());
            }
            
            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<bool>.FromServiceResult(userResult.ToFailureResult<bool>());
            }
            
            var validScene = await sceneValidationService.ValidateSceneWithUserAsync(sceneRemoveDTO.SceneId, userResult.Data.Id);
            if (validScene.IsFailure)
            {
                return HttpServiceResult<bool>.FromServiceResult(
                    validScene.ToFailureResult<bool>());
            }
            
            try
            {
                var deleteSceneAudioResult = await sceneAudioService.RemoveAllAudioForSceneAsync(sceneRemoveDTO.ToSceneAudioRemoveAllDtoFromRemoveDTO(), user);
                if (deleteSceneAudioResult.IsFailure)
                {
                    return HttpServiceResult<bool>.FromServiceResult(
                        deleteSceneAudioResult.ToFailureResult<bool>());
                }

                // Delete the scene
                var deleteSceneResult = await helper.DeleteSceneAsync(sceneRemoveDTO, userResult.Data);
                if (deleteSceneResult.IsFailure)
                {
                    return HttpServiceResult<bool>.FromServiceResult(
                        deleteSceneResult.ToFailureResult<bool>());
                }

                return HttpServiceResult<bool>.FromServiceResult(
                    ServiceResult<bool>.SuccessResult(true, MessageKey.Success_SceneRemoval));
            }
            catch
            {
                return HttpServiceResult<bool>.FromServiceResult(
                    ServiceResult<bool>.Failure(MessageKey.Error_InternalServerError));
            }
        }
    }
}
