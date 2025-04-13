using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Controllers;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;

namespace TTT2.Controllers
{
    [Authorize]
    [Route("sceneaudio")]
    [ApiController]
    public class SceneAudioController(ISceneAudioService sceneAudioService, IHttpResponseConverter responseConverter) : ControllerBase
    {
        [HttpPost("assign-audio")]
        public async Task<IActionResult> AssignAudio([FromBody] SceneAudioAssignDTO sceneAudioAssignDTO)
        {
            var result = await sceneAudioService.AssignAudio(sceneAudioAssignDTO, User);
            return responseConverter.Convert(HttpServiceResult<SceneAudioAssignResponseDTO>.FromServiceResult(result));
        }

        [HttpDelete("remove-audio")]
        public async Task<IActionResult> RemoveAudio([FromBody] SceneAudioRemoveDTO sceneAudioRemoveDTO)
        {
            var result = await sceneAudioService.RemoveAudio(sceneAudioRemoveDTO, User);
            return responseConverter.Convert(HttpServiceResult<bool>.FromServiceResult(result));
        }
        
        [HttpDelete("remove-all-audio")]
        public async Task<IActionResult> RemoveAllAudio([FromBody] SceneAudioRemoveAllDTO sceneAudioRemoveAllDTO)
        {
            var result = await sceneAudioService.RemoveAllAudioForSceneAsync(sceneAudioRemoveAllDTO, User);
            return responseConverter.Convert(HttpServiceResult<bool>.FromServiceResult(result));
        }

        [HttpGet("get-scene-audio")]
        public async Task<IActionResult> GetSceneAudio([FromBody] SceneAudioGetDTO sceneAudioGetDTO)
        {
            var result = await sceneAudioService.GetSceneAudioFilesBySceneIdAsync(sceneAudioGetDTO, User);
            return responseConverter.Convert(HttpServiceResult<List<SceneAudioFile>>.FromServiceResult(result));
        }
    }
}