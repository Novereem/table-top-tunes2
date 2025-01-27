using System.Security.Claims;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;

namespace Shared.Interfaces.Services;

public interface IAudioService
{
    Task<HttpServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, ClaimsPrincipal user);
    Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId);
}