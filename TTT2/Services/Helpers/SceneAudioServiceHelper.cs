using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers
{
    public class SceneAudioServiceHelper(ISceneAudioData sceneAudioData) : ISceneAudioServiceHelper
    {
        public async Task<ServiceResult<SceneAudioAssignResponseDTO>> AddSceneAudioFileAsync(SceneAudioAssignDTO sceneAudioAssignDTO)
        {
            try
            {
                var assignedAudioScene = await sceneAudioData.AddSceneAudioFileAsync(sceneAudioAssignDTO.ToSceneAudioFileFromAssignDTO());
                return assignedAudioScene.ResultType switch
                {
                    DataResultType.Error => ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey
                        .Error_InternalServerErrorData),
                    DataResultType.AlreadyExists => ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey
                        .Error_SceneAudioAlreadyAdded),
                    DataResultType.Success => ServiceResult<SceneAudioAssignResponseDTO>.SuccessResult(
                        assignedAudioScene.Data!.ToSceneAudioAssignDTO()),
                    _ => ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_InternalServerError)
                };
            }
            catch
            {
                return ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData);
            }
        }

        public async Task<ServiceResult<bool>> RemoveSceneAudioFileAsync(SceneAudioRemoveDTO sceneAudioRemoveDTO)
        {
            try
            {
                var removedSceneAudio =
                    await sceneAudioData.RemoveSceneAudioFileAsync(sceneAudioRemoveDTO.ToSceneAudioFileFromRemoveDTO());
                return removedSceneAudio.ResultType switch
                {
                    DataResultType.Success => ServiceResult<bool>.SuccessResult(true),
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
        
        public async Task<ServiceResult<List<SceneAudioFile>>> GetSceneAudioFilesAsync(Guid sceneId)
        {
            try
            {
                var sceneAudioFiles = await sceneAudioData.GetSceneAudioFilesBySceneIdAsync(sceneId);
                return sceneAudioFiles.ResultType switch
                {
                    DataResultType.Success => ServiceResult<List<SceneAudioFile>>.SuccessResult(sceneAudioFiles.Data),
                    DataResultType.NotFound => ServiceResult<List<SceneAudioFile>>.SuccessResult([]),
                    DataResultType.Error => ServiceResult<List<SceneAudioFile>>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<List<SceneAudioFile>>.Failure(MessageKey.Error_InternalServerErrorData),
                };
            }
            catch
            {
                return ServiceResult<List<SceneAudioFile>>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }
    }
}
