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
    public class AuthenticationService(
        IAuthenticationServiceHelper helper,
        IPasswordHashingService passwordHashingService,
        IAuthenticationData authData)
        : IAuthenticationService
    {
        public async Task<HttpServiceResult<RegisterResponseDTO>> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var validationResult = await helper.ValidateRegistrationAsync(registerDTO);
            if (validationResult.IsFailure)
            {
                return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(validationResult.ToFailureResult<RegisterResponseDTO>());
            }

            var newUser = registerDTO.ToUserFromRegisterDTO(passwordHashingService);
            await authData.RegisterUserAsync(newUser);

            return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(
                ServiceResult<RegisterResponseDTO>.SuccessResult(newUser.ToRegisterResponseDTO(), MessageKey.Success_Register)
            );
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
                var token = helper.GenerateJwtToken(user!.Id, user.Username);
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
                return HttpServiceResult<LoginResponseDTO>.FromServiceResult(ServiceResult<LoginResponseDTO>.Failure(MessageKey.Error_InternalServerError));
            }
        }
        
        public async Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
        {
            return await helper.GetUserByIdAsync(userId);
        }
    }
}