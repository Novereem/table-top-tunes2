using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers
{
    public class SceneServiceHelper(ISceneData sceneData) : ISceneServiceHelper
    {
        public ServiceResult<object> ValidateSceneCreate(SceneCreateDTO sceneDTO)
        {
            return string.IsNullOrWhiteSpace(sceneDTO.Name)
                ? ServiceResult<object>.Failure(MessageKey.Error_InvalidInput)
                : ServiceResult<object>.SuccessResult();
        }

        public async Task<ServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, User user)
        {
            try
            {
                var newScene = sceneDTO.ToSceneFromCreateDTO();
                newScene.UserId = user.Id;

                var createdScene = await sceneData.CreateSceneAsync(newScene);

                return createdScene.ResultType switch
                {
                    DataResultType.Success => ServiceResult<SceneCreateResponseDTO>.SuccessResult(createdScene.Data!.ToCreateResponseDTO()),
                    DataResultType.Error => ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        public async Task<ServiceResult<List<Scene>>> RetrieveScenesByUserIdAsync(Guid userId)
        {
            try
            {
                var scenes = await sceneData.GetScenesByUserIdAsync(userId);

                return scenes.ResultType switch
                {
                    DataResultType.Success => ServiceResult<List<Scene>>.SuccessResult(scenes.Data!),
                    DataResultType.NotFound => ServiceResult<List<Scene>>.SuccessResult([]),
                    DataResultType.Error => ServiceResult<List<Scene>>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<List<Scene>>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<List<Scene>>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        public async Task<ServiceResult<bool>> ValidateSceneWithUserAsync(Guid sceneId, Guid userId)
        {
            try
            {
                var isValid = await sceneData.SceneBelongsToUserAsync(sceneId, userId);

                return isValid.ResultType switch
                {
                    DataResultType.Success => ServiceResult<bool>.SuccessResult(true),
                    DataResultType.NotFound => ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized),
                    DataResultType.Error => ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }
    }
}
