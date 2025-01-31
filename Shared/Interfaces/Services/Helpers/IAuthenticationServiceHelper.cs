using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.DTOs.Authentication;
using Shared.Models;

namespace Shared.Interfaces.Services.Helpers
{
    public interface IAuthenticationServiceHelper
    {
        Task<ServiceResult<User>> ValidateLoginAsync(LoginDTO loginDTO);
        Task<ServiceResult<object>> ValidateRegistrationAsync(RegisterDTO registerDTO);
        Task<ServiceResult<User>> RegisterUserAsync(RegisterDTO registerDTO);
        Task<ServiceResult<User>> GetUserByIdAsync(Guid userId);
        Task<ServiceResult<bool>> ValidateUserUpdateAsync(UpdateUserDTO updateUserDTO, User user);
        Task<ServiceResult<User>> UpdateUserAsync(UpdateUserDTO updateUserDTO, User user);
        ServiceResult<string> GenerateJwtToken(User user);
    }
}
