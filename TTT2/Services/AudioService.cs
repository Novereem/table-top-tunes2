using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.Authentication;

namespace TTT2.Services;

public class AudioService(IUserClaimsService userClaimsService, IAudioServiceHelper helper, IAuthenticationService authenticationService, IUserStorageService userStorageService) : IAudioService
{
    public async Task<HttpServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO audioFileCreateDTO, ClaimsPrincipal user)
    {
        var userIdResult = userClaimsService.GetUserIdFromClaims(user);
        if (userIdResult.IsFailure)
        {
            return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<AudioFileCreateResponseDTO>());
        }
        
        var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
        if (userResult.IsFailure || userResult.Data == null)
        {
            return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(userResult.ToFailureResult<AudioFileCreateResponseDTO>());
        }
        
        try
        {
            var validationResult = helper.ValidateAudioFileCreateRequest(audioFileCreateDTO, userResult.Data);
            if (validationResult.IsFailure)
            {
                return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(validationResult.ToFailureResult<AudioFileCreateResponseDTO>()); 
            }
            
            var increaseStorage = await userStorageService.IncreaseUserStorageAsync(userResult.Data.Id, audioFileCreateDTO.AudioFile.Length);
            if (increaseStorage.IsFailure)
            {
                return HttpServiceResult<AudioFileCreateResponseDTO>.FromServiceResult(
                    increaseStorage.ToFailureResult<AudioFileCreateResponseDTO>());
            }
            
            var createdAudioResult = await helper.CreateAudioFileAsync(audioFileCreateDTO, userResult.Data);
            if (createdAudioResult.IsFailure)
            {
                await userStorageService.DecreaseUserStorageAsync(userResult.Data.Id, audioFileCreateDTO.AudioFile.Length);
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

    public async Task<HttpServiceResult<bool>> DeleteAudioFileAsync(AudioFileRemoveDTO audioFileRemoveDTO, ClaimsPrincipal user)
    {
        var userIdResult = userClaimsService.GetUserIdFromClaims(user);
        if (userIdResult.IsFailure)
        {
            return HttpServiceResult<bool>.FromServiceResult(userIdResult.ToFailureResult<bool>());
        }
        
        var userResult = await authenticationService.GetUserByIdAsync(userIdResult.Data);
        if (userResult.IsFailure || userResult.Data == null)
        {
            return HttpServiceResult<bool>.FromServiceResult(userResult.ToFailureResult<bool>());
        }
        
        try
        {
            var validOwnership = await ValidateAudioFileWithUserAsync(audioFileRemoveDTO.AudioId, userIdResult.Data);
            if (validOwnership.IsFailure)
            {
                return HttpServiceResult<bool>.FromServiceResult(validOwnership.ToFailureResult<bool>());
            }
            
            var audioRemovalResult = await helper.DeleteAudioFileAsync(audioFileRemoveDTO, userResult.Data);
            
            if (audioRemovalResult.IsFailure)
            {
                return HttpServiceResult<bool>.FromServiceResult(audioRemovalResult.ToFailureResult<bool>());
            }

            await userStorageService.DecreaseUserStorageAsync(userResult.Data.Id, audioRemovalResult.Data);
            
            return HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.SuccessResult(true, MessageKey.Success_AudioRemoval));
        }
        catch
        {
            return HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.Failure(MessageKey.Error_InternalServerError));
        }
    }

    public async Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId)
    {
        return await helper.ValidateAudioFileWithUserAsync(audioId, userId);
    }
}