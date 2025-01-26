using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using System.Xml.Linq;

namespace TTT2.Services.Helpers
{
    public class AuthenticationServiceHelper : IAuthenticationServiceHelper
    {
        private readonly IAuthenticationData _authData;

        public AuthenticationServiceHelper(
            IAuthenticationData authData
        )
        {
            _authData = authData;
        }
        public async Task<ServiceResult<object>> ValidateRegistrationAsync(RegisterDTO registerDTO)
        {
            if (await _authData.GetUserByEmailAsync(registerDTO.Email) != null)
            {
                return ServiceResult<object>.Failure(MessageKey.Error_EmailTaken);
            }
            if (string.IsNullOrWhiteSpace(registerDTO.Username) || string.IsNullOrWhiteSpace(registerDTO.Password) || string.IsNullOrWhiteSpace(registerDTO.Email))
            {
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
            }
            if (registerDTO.Password.Length < 5)
            {
                return ServiceResult<object>.Failure(MessageKey.Error_PasswordTooShort);
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(registerDTO.Email);
                if (addr.Address != registerDTO.Email)
                {
                    return ServiceResult<object>.Failure(MessageKey.Error_InvalidEmail);
                }
            } 
            catch
            {
                return ServiceResult<object>.Failure(MessageKey.Error_InvalidEmail);
            }
            return ServiceResult<object>.SuccessResult();
        }
    }
}
