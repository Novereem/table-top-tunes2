using Microsoft.AspNetCore.Http;
using Shared.Models.Common;

namespace Shared.Interfaces.Services.Helpers.FileValidation
{
    public interface IFileSafetyValidator
    {
        Task<ServiceResult<object>> ScanWithClamAV(IFormFile audioFile);
    }
}