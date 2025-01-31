using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Models.DTOs.Authentication;

namespace Shared.Models.Extensions
{
    public static class AuthenticationExtensions
    {
        public static User ToUserFromRegisterDTO(this RegisterDTO dto, IPasswordHashingService passwordHashingService)
        {
            var hashedPassword = passwordHashingService.HashPassword(dto.Password);

            return new User
            {
                Username = dto.Username, 
                Email = dto.Email, 
                PasswordHash = hashedPassword,
                UsedStorageBytes = 0,
                MaxStorageBytes = 52428800
            };
        }

        public static RegisterResponseDTO ToRegisterResponseDTO(this User user)
        {
            return new RegisterResponseDTO
            {
                Username = user.Username
            };
        }

        public static UpdateUserResponseDTO ToUpdateUserResponseDTO(this User user)
        {
            return new UpdateUserResponseDTO
            {
                Username = user.Username,
                Email = user.Email,
                MaxStorageBytes = user.MaxStorageBytes,
                UsedStorageBytes = user.UsedStorageBytes,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
