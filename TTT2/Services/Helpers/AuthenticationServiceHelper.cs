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
            var existingUserEmail = await authData.GetUserByEmailAsync(registerDTO.Email);
            if (existingUserEmail.ResultType == DataResultType.Success)
            {
                return ServiceResult<object>.Failure(MessageKey.Error_EmailTaken);
            }
            var existingUserUsername = await authData.GetUserByUsernameAsync(registerDTO.Username);
            if (existingUserUsername.ResultType == DataResultType.Success)
            {
                return ServiceResult<object>.Failure(MessageKey.Error_UsernameTaken);
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

        public async Task<ServiceResult<bool>> ValidateUserUpdateAsync(UpdateUserDTO updateUserDTO, User user)
        {
            //Validate new username if provided
            if (!string.IsNullOrWhiteSpace(updateUserDTO.Username) || updateUserDTO.Username == user.Username)
            {
                var existingUserWithNewName = await authData.GetUserByUsernameAsync(updateUserDTO.Username);
                if (existingUserWithNewName.ResultType == DataResultType.Success 
                    && existingUserWithNewName.Data!.Id != user.Id)
                {
                    return ServiceResult<bool>.Failure(MessageKey.Error_UsernameTaken);
                }
            }
            
            //Validate new email if provided
            if (!string.IsNullOrWhiteSpace(updateUserDTO.Email) || updateUserDTO.Email == user.Email)
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(updateUserDTO.Email);
                    if (addr.Address != updateUserDTO.Email)
                    {
                        return ServiceResult<bool>.Failure(MessageKey.Error_InvalidEmail);
                    }
                }
                catch
                {
                    return ServiceResult<bool>.Failure(MessageKey.Error_InvalidEmail);
                }

                var existingUserWithNewEmail = await authData.GetUserByEmailAsync(updateUserDTO.Email);
                if (existingUserWithNewEmail.ResultType == DataResultType.Success 
                    && existingUserWithNewEmail.Data!.Id != user.Id)
                {
                    return ServiceResult<bool>.Failure(MessageKey.Error_EmailTaken);
                }
            }
            
            //Validate password
            if (!string.IsNullOrWhiteSpace(updateUserDTO.NewPassword))
            {
                if (!passwordHashingService.VerifyPassword(updateUserDTO.OldPassword, user.PasswordHash))
                {
                    return ServiceResult<bool>.Failure(MessageKey.Error_InvalidOldPassword);
                }

                if (updateUserDTO.NewPassword.Length < 5)
                {
                    return ServiceResult<bool>.Failure(MessageKey.Error_PasswordTooShort);
                }
            }
            return ServiceResult<bool>.SuccessResult(true);
        }
        
        public async Task<ServiceResult<User>> UpdateUserAsync(UpdateUserDTO updateUserDTO, User user)
        {
            if (!string.IsNullOrWhiteSpace(updateUserDTO.Username))
                user.Username = updateUserDTO.Username;

            if (!string.IsNullOrWhiteSpace(updateUserDTO.Email))
                user.Email = updateUserDTO.Email;

            if (!string.IsNullOrWhiteSpace(updateUserDTO.NewPassword))
                user.PasswordHash = passwordHashingService.HashPassword(updateUserDTO.NewPassword);

            if (updateUserDTO.UsedStorageBytes.HasValue)
                user.UsedStorageBytes = updateUserDTO.UsedStorageBytes.Value;

            if (updateUserDTO.MaxStorageBytes.HasValue && updateUserDTO.MaxStorageBytes == 52428800 )
                user.MaxStorageBytes = updateUserDTO.MaxStorageBytes.Value;
            try
            {
                var updateResult = await authData.UpdateUserAsync(user);
                return updateResult.ResultType switch
                {
                    DataResultType.Success => ServiceResult<User>.SuccessResult(updateResult.Data!),
                    DataResultType.NotFound => ServiceResult<User>.Failure(MessageKey.Error_NotFound),
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
            try
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
                    expires: DateTime.UtcNow.AddMinutes(1440),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return ServiceResult<string>.SuccessResult(tokenString);
            }
            catch
            {
                return ServiceResult<string>.Failure(MessageKey.Error_JWTNullOrEmpty);
            }
        }
    }
}
