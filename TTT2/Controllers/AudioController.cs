using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Controllers;
using Shared.Interfaces.Services;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.Authentication;

namespace TTT2.Controllers
{
    [Authorize]
    [Route("audio")]
    [ApiController]
    public class AudioController(IAudioService audioService, IHttpResponseConverter responseConverter) : ControllerBase
    {
        [HttpPost("create-audio")]
        public async Task<IActionResult> CreateAudio([FromForm] AudioFileCreateDTO audioFileCreateDTO)
        {
            var result = await audioService.CreateAudioFileAsync(audioFileCreateDTO, User);
            return responseConverter.Convert(HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(result));
        }
        
        [HttpDelete("remove-audio")]
        public async Task<IActionResult> RemoveAudio([FromBody] AudioFileRemoveDTO audioFileRemoveDTO)
        {
            var result = await audioService.DeleteAudioFileAsync(audioFileRemoveDTO, User);
            return responseConverter.Convert(HttpServiceResult<bool>.FromServiceResult(result));
        }
    }
}