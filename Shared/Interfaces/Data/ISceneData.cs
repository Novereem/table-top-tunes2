using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Common;

namespace Shared.Interfaces.Data
{
    public interface ISceneData
    {
        Task<DataResult<Scene>> CreateSceneAsync(Scene scene);
        Task<DataResult<List<Scene>>> GetScenesByUserIdAsync(Guid userId);
        Task<DataResult<bool>> SceneBelongsToUserAsync(Guid sceneId, Guid userId);
    }
}
