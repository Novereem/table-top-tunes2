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
        private readonly string _audioFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        public ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO audioFileCreateDTO)
        {
             if (string.IsNullOrWhiteSpace(audioFileCreateDTO.Name))
                 return ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
             if (audioFileCreateDTO.AudioFile.Length <= 0)
                 return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFile);
             return !audioFileCreateDTO.AudioFile.ContentType.Contains("audio/mpeg") ? 
                 ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType) : 
                 ServiceResult<object>.SuccessResult();
        }

        public async Task<ServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, User user)
        {
            string? filePath = null;
            
            try
            {
                var newAudioFile = audioFileCreateDTO.ToAudioFromCreateDTO();
                
                //Saving audio file
                if (!Directory.Exists(_audioFolderPath))
                    Directory.CreateDirectory(_audioFolderPath);
                
                var userFolderPath = Path.Combine(_audioFolderPath, user.Id.ToString());
                if (!Directory.Exists(userFolderPath))
                    Directory.CreateDirectory(userFolderPath);
            
                var fileName = $"{newAudioFile.Id}.mp3";
                filePath = Path.Combine(userFolderPath, Path.GetFileName(fileName));

                try
                {
                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await audioFileCreateDTO.AudioFile.CopyToAsync(stream);
                }
                catch
                {
                    return ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_UnableToUploadAudioFile);
                }
                
                //Saving metadata to database
                newAudioFile.UserId = user.Id;
                var createdAudio = await audioData.SaveAudioFileAsync(newAudioFile);

                if (createdAudio.ResultType == DataResultType.Success)
                {
                    return ServiceResult<AudioFileCreateResponseDTO>.SuccessResult(createdAudio.Data!.ToCreateResponseDTO());
                }

                //Delete file upon failing to save to database.
                File.Delete(filePath);

                return ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_UnableToSaveAudioFileMetaData);
            }
            catch
            {
                if (filePath != null && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
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