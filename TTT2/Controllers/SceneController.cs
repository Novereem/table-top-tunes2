using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.Scenes;
using Shared.Interfaces.Controllers;
using Shared.Models.Common;

namespace TTT2.Controllers
{
    [Authorize]
    [Route("scenes")]
    [ApiController]
    public class SceneController(ISceneService sceneService, IHttpResponseConverter responseConverter) : ControllerBase
    {
        [HttpPost("create-scene")]
        public async Task<IActionResult> CreateScene([FromBody] SceneCreateDTO sceneCreateDTO)
        {
            var result = await sceneService.CreateSceneAsync(sceneCreateDTO, User);
            return responseConverter.Convert(HttpServiceResult<SceneCreateResponseDTO>.FromServiceResult(result));
        }

        [HttpGet("get-scene")]
        public async Task<IActionResult> GetSceneById([FromBody] SceneGetDTO sceneGetDTO)
        {
            var result = await sceneService.GetSceneByIdAsync(sceneGetDTO, User);
            return responseConverter.Convert(HttpServiceResult<SceneGetResponseDTO>.FromServiceResult(result));
        }
        
        [HttpGet("get-scenes")]
        public async Task<IActionResult> GetScenesList()
        {
            var result = await sceneService.GetScenesListByUserIdAsync(User);
            return responseConverter.Convert(HttpServiceResult<List<SceneListItemDTO>>.FromServiceResult(result));
        }
        
        [HttpPut("update-scene")]
        public async Task<IActionResult> UpdateScene([FromBody] SceneUpdateDTO sceneUpdateDTO)
        {
            var result = await sceneService.UpdateSceneAsync(sceneUpdateDTO, User);
            return responseConverter.Convert(HttpServiceResult<SceneUpdateResponseDTO>.FromServiceResult(result));
        }
        
        [HttpDelete("remove-scene")]
        public async Task<IActionResult> DeleteScene([FromBody] SceneRemoveDTO sceneRemoveDTO)
        {
            var result = await sceneService.DeleteSceneAsync(sceneRemoveDTO, User);
            return responseConverter.Convert(HttpServiceResult<bool>.FromServiceResult(result));
        }
    }
}
