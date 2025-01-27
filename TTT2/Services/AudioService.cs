using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;

namespace TTT2.Services;

public class AudioService(IUserClaimsService userClaimsService, IAudioServiceHelper helper, IAuthenticationService authenticationService) : IAudioService
{
    public async Task<HttpServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, ClaimsPrincipal user)
    {
        var userIdResult = userClaimsService.GetUserIdFromClaims(user);
        if (userIdResult.IsFailure)
        {
            return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<AudioFileCreateResponseDTO>());
        }

        var validationResult = helper.ValidateAudioFileCreateRequest(audioFileCreateDTO);
        if (validationResult.IsFailure)
        {
            return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<AudioFileCreateResponseDTO>()); 
        }

        try
        {
            var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<AudioFileCreateResponseDTO>());
            }
            var createdAudioResult = await helper.CreateAudioFileAsync(audioFileCreateDTO, userResult.Data);
            if (createdAudioResult.IsFailure)
            {
                return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<AudioFileCreateResponseDTO>());
            }

            return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(
                ServiceResult<AudioFileCreateResponseDTO>.SuccessResult(createdAudioResult.Data,
                    MessageKey.Success_AudioCreation));
        }
        catch
        {
            return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(
                ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerError));
        }
    }
}