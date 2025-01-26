using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces.Services.Common.Authentication
{
    public interface IUserClaimsService
    {
        ServiceResult<Guid> GetUserIdFromClaims(ClaimsPrincipal user);
    }
}
