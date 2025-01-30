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
using Shared.Models.Extensions;

namespace TTT2.Services.Helpers
{
    public class AuthenticationServiceHelper(
        IAuthenticationData authData,
        IPasswordHashingService passwordHashingService)
        : IAuthenticationServiceHelper
    {
        public async Task<ServiceResult<object>> ValidateRegistrationAsync(RegisterDTO registerDTO)
        {
            var existingUser = await authData.GetUserByEmailAsync(registerDTO.Email);
            if (existingUser.ResultType == DataResultType.Success)
            {
                return ServiceResult<object>.Failure(MessageKey.Error_EmailTaken);
            }

            if (string.IsNullOrWhiteSpace(registerDTO.Username) || 
                string.IsNullOrWhiteSpace(registerDTO.Password) || 
                string.IsNullOrWhiteSpace(registerDTO.Email))
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
                return ServiceResult<object>.SuccessResult();
            }
            catch
            {
                return ServiceResult<object>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }
        
        public async Task<ServiceResult<User>> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var userFromRegisterDTO = registerDTO.ToUserFromRegisterDTO(passwordHashingService);
            try
            {
                var newUser = await authData.RegisterUserAsync(userFromRegisterDTO);
                return newUser.ResultType switch
                {
                    DataResultType.Success => ServiceResult<User>.SuccessResult(newUser.Data),
                    DataResultType.Error => ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        public async Task<ServiceResult<User>> ValidateLoginAsync(LoginDTO loginDTO)
        {
            var user = await authData.GetUserByUsernameAsync(loginDTO.Username);
            try
            {
                if (user.ResultType != DataResultType.Success || 
                    !passwordHashingService.VerifyPassword(loginDTO.Password, user.Data!.PasswordHash))
                {
                    return ServiceResult<User>.Failure(MessageKey.Error_InvalidCredentials);
                }
                return ServiceResult<User>.SuccessResult(user.Data!);
            }
            catch
            {
                return ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        
        public async Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await authData.GetUserByIdAsync(userId);

                return user.ResultType switch
                {
                    DataResultType.Success => ServiceResult<User>.SuccessResult(user.Data!, MessageKey.Success_DataRetrieved),
                    DataResultType.NotFound => ServiceResult<User>.Failure(MessageKey.Error_Unauthorized),
                    DataResultType.Error => ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData),
                    _ => ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData)
                };
            }
            catch
            {
                return ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
            }
        }

        public ServiceResult<string> GenerateJwtToken(User user)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(secretKey))
            {
                return ServiceResult<string>.Failure(MessageKey.Error_JWTNullOrEmpty);
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
