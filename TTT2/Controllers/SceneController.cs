using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.Scenes;
using Shared.Extensions.Controllers;

namespace TTT2.Controllers
{
    [Authorize]
    [ApiController]
    public class SceneController : ControllerBase
    {
        private readonly ISceneService _sceneService;

        public SceneController(ISceneService sceneService)
        {
            _sceneService = sceneService;
        }

        [HttpPost("create-scene")]
        public async Task<IActionResult> CreateScene([FromBody] SceneCreateDTO sceneDTO)
        {
            var result = await _sceneService.CreateSceneAsync(sceneDTO, User);
            return this.ToActionResult(result);
        }

        [HttpGet("get-scenes")]
        public async Task<IActionResult> GetScenesList(Guid id)
        {
            var result = await _sceneService.GetScenesListByUserIdAsync(id, User);
            return this.ToActionResult(result);
        }
    }
}
