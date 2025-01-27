using Microsoft.IdentityModel.Tokens;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace TTT2.Services.Helpers
{
    public class AuthenticationServiceHelper(
        IAuthenticationData authData,
        IPasswordHashingService passwordHashingService)
        : IAuthenticationServiceHelper
    {
        public async Task<ServiceResult<object>> ValidateRegistrationAsync(RegisterDTO registerDTO)
        {
            if (await authData.GetUserByEmailAsync(registerDTO.Email) != null)
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

        public async Task<ServiceResult<User>> ValidateLoginAsync(LoginDTO loginDTO)
        {
            var user = await authData.GetUserByUsernameAsync(loginDTO.Username);
            if (user == null || !passwordHashingService.VerifyPassword(loginDTO.Password, user.PasswordHash))
            {
                return ServiceResult<User>.Failure(MessageKey.Error_InvalidCredentials);
            }

            return ServiceResult<User>.SuccessResult(user);
        }
        
        public async Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await authData.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return ServiceResult<User>.Failure(MessageKey.Error_Unauthorized);
                }

                return ServiceResult<User>.SuccessResult(user, MessageKey.Success_DataRetrieved);
            }
            catch
            {
                return ServiceResult<User>.Failure(MessageKey.Error_InternalServerError);
            }
        }

        public ServiceResult<string> GenerateJwtToken(Guid userGuid, string username)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(secretKey))
            {
                return ServiceResult<string>.Failure(MessageKey.Error_JWTNullOrEmpty);
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, userGuid.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
                audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(1440),
                signingCredentials: creds
            );

            return ServiceResult<string>.SuccessResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
