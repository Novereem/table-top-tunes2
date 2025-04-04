﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Extensions.Controllers;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.AudioFiles;

namespace TTT2.Controllers
{
    [Authorize]
    [Route("audio")]
    [ApiController]
    public class AudioController(IAudioService audioService) : ControllerBase
    {
        [HttpPost("create-audio")]
        public async Task<IActionResult> CreateAudio([FromForm] AudioFileCreateDTO audioFileCreateDTO)
        {
            var result = await audioService.CreateAudioFileAsync(audioFileCreateDTO, User);
            return this.ToActionResult(result);
        }
        [HttpDelete("remove-audio")]
        public async Task<IActionResult> RemoveAudio([FromBody] AudioFileRemoveDTO audioFileRemoveDTO)
        {
            var result = await audioService.DeleteAudioFileAsync(audioFileRemoveDTO, User);
            return this.ToActionResult(result);
        }
    }
}