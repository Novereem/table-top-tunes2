using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers
{
    public class SceneServiceHelper(ISceneData sceneData, ISceneValidationService sceneValidationService) : ISceneServiceHelper
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
        
        public async Task<ServiceResult<SceneGetResponseDTO>> RetrieveSceneBySceneIdAsync(SceneGetDTO sceneDTO)
        {
            try
            {
                var scenes = await sceneData.GetSceneBySceneIdAsync(sceneDTO.SceneId);

                return scenes.ResultType switch
                {
                    DataResultType.Success => ServiceResult<SceneGetResponseDTO>.SuccessResult(scenes.Data!.ToGetResponseDTO()),
                    DataResultType.NotFound => ServiceResult<SceneGetResponseDTO>.Failure(MessageKey.Error_NotFound),
                    DataResultType.Error => ServiceResult<SceneGetResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<SceneGetResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<SceneGetResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        public async Task<ServiceResult<List<Scene>>> RetrieveScenesByUserIdAsync(User user)
        {
            try
            {
                var scenes = await sceneData.GetScenesByUserIdAsync(user.Id);

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
        
        public ServiceResult<object> ValidateSceneUpdate(SceneUpdateDTO sceneUpdateDTO)
        {
            return sceneUpdateDTO.SceneId == Guid.Empty || string.IsNullOrWhiteSpace(sceneUpdateDTO.NewName)
                ? ServiceResult<object>.Failure(MessageKey.Error_InvalidInput)
                : ServiceResult<object>.SuccessResult();
        }
        
        public async Task<ServiceResult<SceneUpdateResponseDTO>> UpdateSceneAsync(SceneUpdateDTO sceneUpdateDTO, User user)
        {
            try
            {
                var validSceneResult = await sceneValidationService.ValidateSceneWithUserAsync(sceneUpdateDTO.SceneId, user.Id);
                if (validSceneResult.IsFailure)
                {
                    return validSceneResult.ToFailureResult<SceneUpdateResponseDTO>();
                }

                var sceneResult = await RetrieveSceneBySceneIdAsync(new SceneGetDTO { SceneId = sceneUpdateDTO.SceneId });
                if (sceneResult.IsFailure)
                {
                    return sceneResult.ToFailureResult<SceneUpdateResponseDTO>();
                }
                
                sceneResult.Data!.Name = sceneUpdateDTO.NewName;
                
                var updateResult = await sceneData.UpdateSceneAsync(sceneResult.Data!.ToSceneFromGetResponseDTO());
                if (updateResult.ResultType != DataResultType.Success || updateResult.Data == null)
                {
                    return ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData);
                }

                return updateResult.ResultType switch
                {
                    DataResultType.Success => ServiceResult<SceneUpdateResponseDTO>.SuccessResult(updateResult.Data.ToUpdateResponseDTO()),
                    DataResultType.AlreadyExists => ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_NotFound),
                    DataResultType.Error => ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }
        
        public async Task<ServiceResult<bool>> DeleteSceneAsync(SceneRemoveDTO sceneRemoveDTO, User user)
        {
            try
            {
                var deleteResult = await sceneData.DeleteSceneAsync(sceneRemoveDTO.SceneId, user.Id);

                return deleteResult.ResultType switch
                {
                    DataResultType.Success => ServiceResult<bool>.SuccessResult(),
                    DataResultType.NotFound => ServiceResult<bool>.Failure(MessageKey.Error_NotFound),
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
