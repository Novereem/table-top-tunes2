using Microsoft.AspNetCore.Identity;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using Shared.Models.Extensions;

namespace TTT2.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationServiceHelper _helper;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IAuthenticationData _authData;

        public AuthenticationService(
            IAuthenticationServiceHelper helper,
            IPasswordHashingService passwordHashingService,
            IAuthenticationData authData
        )
        {
            _helper = helper;
            _passwordHashingService = passwordHashingService;
            _authData = authData;
        }

        public async Task<HttpServiceResult<RegisterResponseDTO>> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var validationResult = await _helper.ValidateRegistrationAsync(registerDTO);
            if (!validationResult.IsSuccess)
            {
                return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(validationResult.ToFailureResult<RegisterResponseDTO>());
            }

            var newUser = registerDTO.ToUserFromRegisterDTO(_passwordHashingService);
            await _authData.RegisterUserAsync(newUser);

            return HttpServiceResult<RegisterResponseDTO>.FromServiceResult(
                ServiceResult<RegisterResponseDTO>.SuccessResult(newUser.ToRegisterResponseDTO(), MessageKey.Success_OperationCompleted)
            );
        }
    }
}
