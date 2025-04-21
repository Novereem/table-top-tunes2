using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioStreaming;

namespace TTT2.Services
{
    public class AudioStreamingService(IUserClaimsService userClaimsService, IAudioStreamingServiceHelper helper) : IAudioStreamingService
    {
        public async Task<HttpServiceResult<AudioStreamResponseDTO>> StreamAudioAsync(AudioStreamDTO audioStreamDTO, ClaimsPrincipal user, string? rangeHeader)
        {
            //Validate User
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<AudioStreamResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<AudioStreamResponseDTO>());
            }
            var userId = userIdResult.Data;

            try
            {
                //Validate & Build File Path
                var pathResult = helper.ValidateAndBuildPath(audioStreamDTO.AudioId, userId);
                if (pathResult.IsFailure)
                {
                    return HttpServiceResult<AudioStreamResponseDTO>
                        .FromServiceResult(pathResult.ToFailureResult<AudioStreamResponseDTO>());
                }
                
                var (path, size) = pathResult.Data;
                
                //Choose full vs partial
                var dtoResult = string.IsNullOrEmpty(rangeHeader)
                    ? helper.BuildFullStream(path, size)
                    : helper.BuildPartialStream(audioStreamDTO.AudioId, path, size, rangeHeader);

                return HttpServiceResult<AudioStreamResponseDTO>.FromServiceResult(dtoResult.IsFailure 
                    ? dtoResult.ToFailureResult<AudioStreamResponseDTO>()
                    : ServiceResult<AudioStreamResponseDTO>.SuccessResult(dtoResult.Data));
            }
            catch
            {
                return HttpServiceResult<AudioStreamResponseDTO>
                    .FromServiceResult(ServiceResult<AudioStreamResponseDTO>
                        .Failure(MessageKey.Error_InternalServerError));
            }
        }
    }
}