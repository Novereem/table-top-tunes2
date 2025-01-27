using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.DTOs.Scenes;
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
                if (assignedAudioScene == null)
                {
                    return ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_InternalServerError);
                }
                return ServiceResult<SceneAudioAssignResponseDTO>.SuccessResult(assignedAudioScene.ToSceneAudioAssignDTO());
            }
            catch
            {
                return ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_InternalServerError);
            }
        }
    }
}
