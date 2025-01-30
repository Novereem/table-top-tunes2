using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers
{
    public class AudioServiceHelper(IAudioData audioData) : IAudioServiceHelper
    {
        public ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO audioFileCreateDTO)
        {
            return string.IsNullOrWhiteSpace(audioFileCreateDTO.Name)
                ? ServiceResult<object>.Failure(MessageKey.Error_InvalidInput)
                : ServiceResult<object>.SuccessResult();
        }

        public async Task<ServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, User user)
        {
            try
            {
                var newAudioFile = audioFileCreateDTO.ToAudioFromCreateDTO();
                newAudioFile.UserId = user.Id;

                var createdAudio = await audioData.SaveAudioFileAsync(newAudioFile);

                return createdAudio.ResultType switch
                {
                    DataResultType.Success => ServiceResult<AudioFileCreateResponseDTO>.SuccessResult(createdAudio.Data!.ToCreateResponseDTO()),
                    DataResultType.Error => ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        public async Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId)
        {
            try
            {
                var isValid = await audioData.AudioFileBelongsToUserAsync(audioId, userId);

                return isValid.ResultType switch
                {
                    DataResultType.Success => ServiceResult<bool>.SuccessResult(true),
                    DataResultType.NotFound => ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized),
                    DataResultType.Error => ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }
    }
}