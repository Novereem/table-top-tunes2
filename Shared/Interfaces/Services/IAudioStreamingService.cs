using System.Security.Claims;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioStreaming;

namespace Shared.Interfaces.Services
{
    public interface IAudioStreamingService
    {
        Task<HttpServiceResult<AudioStreamResponseDTO>>
            StreamAudioAsync(AudioStreamDTO audioStreamDTO, ClaimsPrincipal user, string? rangeHeader);
    }
}