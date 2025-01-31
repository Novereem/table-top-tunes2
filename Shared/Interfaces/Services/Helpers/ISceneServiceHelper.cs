using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.Services.Helpers
{
    public interface ISceneServiceHelper
    {
        ServiceResult<object> ValidateSceneCreate(SceneCreateDTO sceneDTO);
        Task<ServiceResult<SceneGetResponseDTO>> RetrieveSceneBySceneIdAsync(SceneGetDTO sceneDTO);
        Task<ServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, User user);
        Task<ServiceResult<List<Scene>>> RetrieveScenesByUserIdAsync(User user);
        ServiceResult<object> ValidateSceneUpdate(SceneUpdateDTO sceneUpdateDTO);
        Task<ServiceResult<SceneUpdateResponseDTO>> UpdateSceneAsync(SceneUpdateDTO sceneUpdateDTO, User user);
        Task<ServiceResult<bool>> DeleteSceneAsync(SceneRemoveDTO sceneRemoveDTO, User user);
    }
}
