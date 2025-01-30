using System.Diagnostics;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services.Helpers.FileValidation;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.Extensions;
using TTT2.Services.Helpers.FileValidation;

namespace TTT2.Services.Helpers
{
    public class AudioServiceHelper(IAudioData audioData, IAudioFileValidator audioFileValidator, IFileSafetyValidator fileSafetyValidator) : IAudioServiceHelper
    {
        private readonly string _audioFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        
        public ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO audioFileCreateDTO)
        {
            var basicCheck = audioFileValidator.ValidateFileBasics(audioFileCreateDTO);
            if (!basicCheck.IsSuccess) return basicCheck;
        
            var magicCheck = audioFileValidator.ValidateMagicNumber(audioFileCreateDTO.AudioFile);
            if (!magicCheck.IsSuccess) return magicCheck;
        
            var decodeCheck = audioFileValidator.ValidateByDecodingWithFfmpeg(audioFileCreateDTO.AudioFile);
            if (!decodeCheck.IsSuccess) return decodeCheck;

            var virusCheck = fileSafetyValidator.ScanWithClamAV(audioFileCreateDTO.AudioFile).GetAwaiter().GetResult();
            if (!virusCheck.IsSuccess) return virusCheck;
        
            return ServiceResult<object>.SuccessResult();
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
                    await using var uploadStream = new FileStream(filePath, FileMode.Create);
                    await using var freshStream = audioFileCreateDTO.AudioFile.OpenReadStream();
                    await freshStream.CopyToAsync(uploadStream);
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