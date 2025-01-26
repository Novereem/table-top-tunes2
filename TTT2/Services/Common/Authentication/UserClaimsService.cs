using Shared.Enums;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Models;
using Shared.Models.Common;
using System.Security.Claims;

namespace TTT2.Services.Common.Authentication
{
    public class UserClaimsService : IUserClaimsService
    {
        public ServiceResult<Guid> GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
            }

            if (!Guid.TryParse(userIdClaim!.Value, out var userId))
            {
                return ServiceResult<Guid>.Failure(MessageKey.Error_Unauthorized);
            }

            return ServiceResult<Guid>.SuccessResult(userId);
        }
    }
}
