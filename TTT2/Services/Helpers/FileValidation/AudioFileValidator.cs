using System.Diagnostics;
using Shared.Enums;
using Shared.Interfaces.Services.Helpers.FileValidation;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;

namespace TTT2.Services.Helpers.FileValidation
{
    public class AudioFileValidator : IAudioFileValidator
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public ServiceResult<object> ValidateFileBasics(AudioFileCreateDTO dto)
        {
            var file = dto.AudioFile;
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
            if (file.Length <= 0)
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFile);
            if (file.Length > MaxFileSizeBytes)
                return ServiceResult<object>.Failure(MessageKey.Error_FileTooLarge);

            var fileExtension = Path.GetExtension(file.FileName);
            if (!string.Equals(fileExtension, ".mp3", StringComparison.OrdinalIgnoreCase))
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);

            return ServiceResult<object>.SuccessResult();
        }

        public ServiceResult<object> ValidateMagicNumber(IFormFile audioFile)
        {
            try
            {
                using var readStream = audioFile.OpenReadStream();
                var buffer = new byte[3];
                var bytesRead = readStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < 3) return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
                if (buffer[0] != 'I' || buffer[1] != 'D' || buffer[2] != '3')
                    return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
            }
            catch
            {
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
            }

            return ServiceResult<object>.SuccessResult();
        }

        public ServiceResult<object> ValidateByDecodingWithFfmpeg(IFormFile audioFile)
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var fs = new FileStream(tempFilePath, FileMode.Create))
                    audioFile.CopyTo(fs);

                var isMp3Valid = RunFfmpegProbe(tempFilePath);
                File.Delete(tempFilePath);

                if (!isMp3Valid)
                    return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
            }
            catch
            {
                File.Delete(tempFilePath);
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
            }

            return ServiceResult<object>.SuccessResult();
        }

        private static bool RunFfmpegProbe(string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-v error -i \"{filePath}\" -f null -",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }
}