using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers
{
    public class SceneServiceHelper : ISceneServiceHelper
    {
        private readonly ISceneData _sceneData;

        public SceneServiceHelper(ISceneData sceneData) {
            _sceneData = sceneData;
        }
        public ServiceResult<object> ValidateSceneCreate(SceneCreateDTO sceneDTO)
        {
            if (string.IsNullOrWhiteSpace(sceneDTO.Name))
            {
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
            }

            return ServiceResult<object>.SuccessResult();
        }

        public async Task<ServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, User user)
        {
            try
            {
                var newScene = sceneDTO.ToSceneFromCreateDTO();
                newScene.UserId = user.Id;

                var createdScene = await _sceneData.CreateSceneAsync(newScene);
                if (createdScene == null)
                {
                    return ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerError);
                }
                return ServiceResult<SceneCreateResponseDTO>.SuccessResult(createdScene.ToCreateResponseDTO());
            }
            catch
            {
                return ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerError);
            }
        }
    }
}
