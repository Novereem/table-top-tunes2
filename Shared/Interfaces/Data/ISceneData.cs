using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.Data
{
    public interface ISceneData
    {
        Task<Scene?> CreateSceneAsync(Scene scene);
        Task<List<Scene>> GetScenesByUserIdAsync(Guid userId);
        Task<bool> SceneBelongsToUserAsync(Guid sceneId, Guid userId);
    }
}
