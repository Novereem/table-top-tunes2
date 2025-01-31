using Shared.Models.Common;

namespace Shared.Interfaces.Services.Helpers.Shared
{
    public interface ISceneValidationService
    {
        Task<ServiceResult<bool>> ValidateSceneWithUserAsync(Guid sceneId, Guid userId);
    }
}