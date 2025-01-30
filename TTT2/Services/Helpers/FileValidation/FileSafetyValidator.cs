using nClam;
using Shared.Enums;
using Shared.Interfaces.Services.Helpers.FileValidation;
using Shared.Models.Common;

namespace TTT2.Services.Helpers.FileValidation
{
    public class FileSafetyValidator : IFileSafetyValidator
    {
        public async Task<ServiceResult<object>> ScanWithClamAV(IFormFile audioFile)
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                await using (var fs = new FileStream(tempFilePath, FileMode.Create))
                    await audioFile.CopyToAsync(fs);

                var clamResult = await ScanFileWithClamAV(tempFilePath);
                File.Delete(tempFilePath);

                if (!clamResult)
                    return ServiceResult<object>.Failure(MessageKey.Error_MalwareOrVirusDetected);
            }
            catch
            {
                File.Delete(tempFilePath);
                return ServiceResult<object>.Failure(MessageKey.Error_InternalServerErrorService);
            }

            return ServiceResult<object>.SuccessResult();
        }

        private static async Task<bool> ScanFileWithClamAV(string filePath)
        {
            // Example using nClam library
            var clam = new ClamClient("localhost", 3310);
            var scanResult = await clam.SendAndScanFileAsync(filePath);
            return scanResult.Result == ClamScanResults.Clean;
        }
    }
}