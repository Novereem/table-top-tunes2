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

namespace TTT2.Services
{
    public class SceneService : ISceneService
    {
        private readonly ISceneServiceHelper _helper;
        private readonly ISceneData _sceneData;
        private readonly IUserClaimsService _userClaimsService;
        private readonly IAuthenticationService _authenticationService;

        public SceneService(ISceneServiceHelper helper, ISceneData sceneData, IUserClaimsService userClaimsService, IAuthenticationService authenticationService)
        {
            _helper = helper;
            _sceneData = sceneData;
            _userClaimsService = userClaimsService;
            _authenticationService = authenticationService;
        }

        public async Task<HttpServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, ClaimsPrincipal user)
        {
            var userIdResult = _userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<SceneCreateResponseDTO>());
            }

            var validationResult = _helper.ValidateSceneCreate(sceneDTO);
            if (validationResult.IsFailure)
            {
                return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<SceneCreateResponseDTO>());
            }

            try
            {
                var userResult = await _authenticationService.GetUserByIdAsync(userIdResult.Data);
                if (userResult.IsFailure || userResult.Data == null)
                {
                    return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<SceneCreateResponseDTO>());
                }
                var createdSceneResult = await _helper.CreateSceneAsync(sceneDTO, userResult.Data);
                if (createdSceneResult.IsFailure)
                {
                    return HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<SceneCreateResponseDTO>());
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
    }
}
