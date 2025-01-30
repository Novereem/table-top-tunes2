using Microsoft.AspNetCore.Http;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;

namespace Shared.Interfaces.Services.Helpers.FileValidation
{
    public interface IAudioFileValidator
    {
        ServiceResult<object> ValidateFileBasics(AudioFileCreateDTO dto);
        ServiceResult<object> ValidateMagicNumber(IFormFile audioFile);
        ServiceResult<object> ValidateByDecodingWithFfmpeg(IFormFile audioFile);
    }
}