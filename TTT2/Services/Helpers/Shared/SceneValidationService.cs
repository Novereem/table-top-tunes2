using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models.Common;

namespace TTT2.Services.Helpers.Shared
{
    public class SceneValidationService(ISceneData sceneData) : ISceneValidationService
    {
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