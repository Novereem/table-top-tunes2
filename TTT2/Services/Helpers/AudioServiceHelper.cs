using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers;

public class AudioServiceHelper(IAudioData audioData) : IAudioServiceHelper
{
    public ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO audioFileCreateDTO)
    {
        return string.IsNullOrWhiteSpace(audioFileCreateDTO.Name) ? ServiceResult<object>.Failure(MessageKey.Error_InvalidInput) : ServiceResult<object>.SuccessResult();
    }

    public async Task<ServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, User user)
    {
        try
        {
            var newAudioFile = audioFileCreateDTO.ToAudioFromCreateDTO();
            newAudioFile.UserId = user.Id;

            var createdAudio = await audioData.SaveAudioFileAsync(newAudioFile);
            if (createdAudio == null)
            {
                return ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerError);
            }
            return ServiceResult<AudioFileCreateResponseDTO>.SuccessResult(createdAudio.ToCreateResponseDTO());
        }
        catch
        {
            return ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerError);
        }
    }

    public async Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId)
    {
        try
        {
            var isValid = await audioData.AudioFileBelongsToUserAsync(audioId, userId);
            if (!isValid)
            {
                return ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized);
            }

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch
        {
            return ServiceResult<bool>.Failure(MessageKey.Error_InternalServerError);
        }
    }
}