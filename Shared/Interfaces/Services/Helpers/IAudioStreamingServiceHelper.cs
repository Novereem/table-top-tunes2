using Shared.Models.Common;
using Shared.Models.DTOs.AudioStreaming;

namespace Shared.Interfaces.Services.Helpers
{
    public interface IAudioStreamingServiceHelper
    {
        ServiceResult<(string physicalPath, long fileSize)> ValidateAndBuildPath(Guid audioId, Guid userId);
        ServiceResult<AudioStreamResponseDTO> BuildFullStream(string physicalPath, long fileSize);
        ServiceResult<AudioStreamResponseDTO> BuildPartialStream(Guid audioId, string physicalPath, long totalSize, string rangeHeader);
    }
}