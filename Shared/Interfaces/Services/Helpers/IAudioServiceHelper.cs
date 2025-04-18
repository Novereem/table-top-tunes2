﻿using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;

namespace Shared.Interfaces.Services.Helpers;

public interface IAudioServiceHelper
{
    ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO createDTO, User user);
    Task<ServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, User user);
    Task<ServiceResult<long>> DeleteAudioFileAsync(AudioFileRemoveDTO audioFileRemoveDTO, User user);
    Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId);
}