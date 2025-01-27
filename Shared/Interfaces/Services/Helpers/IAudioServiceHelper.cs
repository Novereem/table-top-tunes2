using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;

namespace Shared.Interfaces.Services.Helpers;

public interface IAudioServiceHelper
{
    ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO createDTO);
    Task<ServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, User user);
}