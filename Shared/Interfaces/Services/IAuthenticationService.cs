using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.Services
{
    public interface IAuthenticationService
    {
        public Task<HttpServiceResult<RegisterResponseDTO>> RegisterUserAsync(RegisterDTO registerDTO);
    }
}
