using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.Scenes;
using Shared.Extensions.Controllers;

namespace TTT2.Controllers
{
    [Authorize]
    [ApiController]
    public class SceneController(ISceneService sceneService) : ControllerBase
    {
        [HttpPost("create-scene")]
        public async Task<IActionResult> CreateScene([FromBody] SceneCreateDTO sceneCreateDTO)
        {
            var result = await sceneService.CreateSceneAsync(sceneCreateDTO, User);
            return this.ToActionResult(result);
        }

        [HttpGet("get-scenes")]
        public async Task<IActionResult> GetScenesList(Guid id)
        {
            var result = await sceneService.GetScenesListByUserIdAsync(id, User);
            return this.ToActionResult(result);
        }
    }
}
