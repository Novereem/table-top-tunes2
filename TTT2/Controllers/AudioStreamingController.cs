using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Controllers;
using Shared.Interfaces.Services;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioStreaming;

namespace TTT2.Controllers
{
    [ApiController]
    [Route("audio-streaming")]
    [Authorize]
    public class AudioStreamingController(IAudioStreamingService audioStreamingService, IHttpResponseConverter responseConverter) : ControllerBase
    {
        [HttpPost("stream")]
        public async Task<IActionResult> StreamAudio([FromBody] AudioStreamDTO audioStreamDTO)
        {
            var result = await audioStreamingService.StreamAudioAsync(audioStreamDTO, User, Request.Headers.Range.FirstOrDefault());

            // On failure, return a standard JSON error
            if (result.IsFailure)
                return responseConverter
                    .Convert(HttpServiceResult<object>.FromServiceResult(result.ToFailureResult<object>()));

            var data = result.Data!;

            // Apply typed headers
            Response.StatusCode = data.StatusCode;
            Response.ContentType = data.ContentType;
            Response.ContentLength = data.ContentLength;
            Response.Headers.AcceptRanges = data.Headers.AcceptRanges;
            if (data.Headers.ContentRange is not null)
                Response.Headers.ContentRange = data.Headers.ContentRange;

            // Finally, stream the file
            return File(data.FileStream, data.ContentType);
        }
    }
}