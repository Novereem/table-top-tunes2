using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<HttpServiceResult<RegisterResponseDTO>> RegisterUserAsync(RegisterDTO registerDTO);
        Task<HttpServiceResult<LoginResponseDTO>> LoginUserAsync(LoginDTO loginDTO);
        Task<HttpServiceResult<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserDTO updateUserDTO, ClaimsPrincipal user);
        Task<ServiceResult<User>> GetUserByIdAsync(Guid userId);
        
    }
}
