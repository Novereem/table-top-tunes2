using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services;
using Shared.Models.Common;
using Shared.Models;
using Shared.Models.DTOs.Scenes;
using System.Security.Claims;
using Shared.Interfaces.Services.Common.Authentication;
using TTT2.Services.Common.Authentication;
using Shared.Models.DTOs.Authentication;
using Microsoft.AspNetCore.Authentication;
using IAuthenticationService = Shared.Interfaces.Services.IAuthenticationService;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Shared.Models.Extensions;

namespace TTT2.Services
{
    public class SceneService(
        ISceneServiceHelper helper,
        IUserClaimsService userClaimsService,
        IAuthenticationService authenticationService)
        : ISceneService
    {
        public async Task<HttpServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<SceneCreateResponseDTO>());
            }

            var validationResult = helper.ValidateSceneCreate(sceneDTO);
            if (validationResult.IsFailure)
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<SceneCreateResponseDTO>());
            }

            try
            {
                var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
                if (userResult.IsFailure || userResult.Data == null)
                {
                    return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(userResult.ToFailureResult<SceneCreateResponseDTO>());
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

        public async Task<HttpServiceResult<List<SceneListItemDTO>>> GetScenesListByUserIdAsync(Guid sceneId, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(userIdResult.ToFailureResult<List<SceneListItemDTO>>());
            }

            try
            {
                var scenesResult = await helper.RetrieveScenesByUserIdAsync(userIdResult.Data);
                var sceneListItems = scenesResult.Data!.Select(scene => scene.ToSceneListItemDTO()).ToList();
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(
                     ServiceResult<List<SceneListItemDTO>>.SuccessResult(sceneListItems, MessageKey.Success_DataRetrieved)
                 );
            }
            catch
            {
                return HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(ServiceResult<List<SceneListItemDTO>>.Failure(MessageKey.Error_InternalServerError));
            }
        }

        public async Task<ServiceResult<bool>> ValidateSceneWithUserAsync(Guid sceneId, Guid userId)
        {
            return await helper.ValidateSceneWithUserAsync(sceneId, userId);
        }
    }
}
