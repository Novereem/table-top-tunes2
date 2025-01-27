using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Extensions.Controllers;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.SceneAudios;

namespace TTT2.Controllers
{
    [Authorize]
    [ApiController]
    public class SceneAudioController(ISceneAudioService sceneAudioService) : ControllerBase
    {
        [HttpPost("assign-audio")]
        public async Task<IActionResult> AssignAudio([FromBody] SceneAudioAssignDTO sceneAudioAssignDTO)
        {
            var result = await sceneAudioService.AssignAudio(sceneAudioAssignDTO, User);
            return this.ToActionResult(result);
        }
    }
}