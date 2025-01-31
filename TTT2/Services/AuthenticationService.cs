using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using Shared.Models.Extensions;

namespace TTT2.Services
{
    public class AuthenticationService(IAuthenticationServiceHelper helper, IUserClaimsService userClaimsService): IAuthenticationService
    {
        public async Task<HttpServiceResult<RegisterResponseDTO>> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var validationResult = await helper.ValidateRegistrationAsync(registerDTO);
            if (validationResult.IsFailure)
            {
                return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(validationResult.ToFailureResult<RegisterResponseDTO>());
            }

            try
            {
                var newRegisteredUser = await helper.RegisterUserAsync(registerDTO);

                if (newRegisteredUser.IsFailure)
                {
                    return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(newRegisteredUser.ToFailureResult<RegisterResponseDTO>());
                }
                return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(
                    ServiceResult<RegisterResponseDTO>.SuccessResult(newRegisteredUser.Data!.ToRegisterResponseDTO(),
                        MessageKey.Success_Register)
                );
            }
            catch
            {
                return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(ServiceResult<RegisterResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService));
            }
        }

        public async Task<HttpServiceResult<LoginResponseDTO>> LoginUserAsync(LoginDTO loginDTO)
        {
            var validationResult = await helper.ValidateLoginAsync(loginDTO);
            if (validationResult.IsFailure)
            {
                return HttpServiceResult<LoginResponseDTO>.FromServiceResult(validationResult.ToFailureResult<LoginResponseDTO>());
            }

            try
            {
                var user = validationResult.Data;
                var token = helper.GenerateJwtToken(user);
                if (token.IsFailure)
                {
                    return HttpServiceResult<LoginResponseDTO>.FromServiceResult(validationResult.ToFailureResult<LoginResponseDTO>());
                }
                return HttpServiceResult<LoginResponseDTO>.FromServiceResult(
                    ServiceResult<LoginResponseDTO>.SuccessResult(new LoginResponseDTO { Token = token.Data! }, MessageKey.Success_Login)
                );
            }
            catch
            {
                return HttpServiceResult<LoginResponseDTO>.FromServiceResult(ServiceResult<LoginResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService));
            }
        }

        public async Task<HttpServiceResult<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserDTO updateUserDTO, ClaimsPrincipal user)
        {
            var userIdResult = userClaimsService.GetUserIdFromClaims(user);
            if (userIdResult.IsFailure)
            {
                return HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(userIdResult.ToFailureResult<UpdateUserResponseDTO>());
            }
            
            var userResult = await GetUserByIdAsync(userIdResult.Data);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(userResult.ToFailureResult<UpdateUserResponseDTO>());
            }

            try
            {
                var validateUserUpdate = await helper.ValidateUserUpdateAsync(updateUserDTO, userResult.Data);
                if (validateUserUpdate.IsFailure)
                {
                    return HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(
                        validateUserUpdate.ToFailureResult<UpdateUserResponseDTO>());
                }
                var updatedUser = await helper.UpdateUserAsync(updateUserDTO, userResult.Data);
                if (updatedUser.IsFailure)
                {
                    return HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(
                        updatedUser.ToFailureResult<UpdateUserResponseDTO>());
                }

                return HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(
                    ServiceResult<UpdateUserResponseDTO>.SuccessResult(updatedUser.Data!.ToUpdateUserResponseDTO(),
                        MessageKey.Success_UpdatedUser));
            }
            catch
            {
                return HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(ServiceResult<UpdateUserResponseDTO>.Failure(MessageKey.Error_InternalServerErrorService));
            }
        }

        public async Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
        {
            return await helper.GetUserByIdAsync(userId);
        }
    }
}