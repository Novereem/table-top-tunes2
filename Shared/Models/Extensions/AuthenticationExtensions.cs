using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Interfaces.Services.Common;
using Shared.Models.DTOs.Authentication;

namespace Shared.Models.Extensions
{
    public static class AuthenticationExtensions
    {
        public static User ToUserFromRegisterDTO(this RegisterDTO dto, IPasswordHashingService passwordHashingService)
        {
            var hashedPassword = passwordHashingService.HashPassword(dto.Password);

            return new User { Username = dto.Username, Email = dto.Email, PasswordHash = hashedPassword };
        }

        public static RegisterResponseDTO ToRegisterResponseDTO(this User user)
        {
            return new RegisterResponseDTO
            {
                Username = user.Username
            };
        }
    }
}
