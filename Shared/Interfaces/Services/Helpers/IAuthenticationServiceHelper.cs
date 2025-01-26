using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.DTOs.Authentication;

namespace Shared.Interfaces.Services.Helpers
{
    public interface IAuthenticationServiceHelper
    {
        Task<ServiceResult<object>> ValidateRegistrationAsync(RegisterDTO registerDTO);
    }
}
