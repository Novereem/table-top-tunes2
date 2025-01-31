using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;

namespace TTT2.Services
{
    public class UserStorageService(IAuthenticationServiceHelper authHelper) : IUserStorageService
    {

        public async Task<ServiceResult<bool>> IncreaseUserStorageAsync(Guid userId, long additionalBytes)
        {
            var userResult = await authHelper.GetUserByIdAsync(userId);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return userResult.ToFailureResult<bool>();
            }
            
            var user = userResult.Data;

            var newUsage = user.UsedStorageBytes + additionalBytes;
            if (newUsage > user.MaxStorageBytes)
            {
                return ServiceResult<bool>.Failure(MessageKey.Error_ExceedsStorageQuota);
            }

            user.UsedStorageBytes = newUsage;

            var updateDto = new UpdateUserDTO
            {
                UsedStorageBytes = user.UsedStorageBytes
            };

            var updateResult = await authHelper.UpdateUserAsync(updateDto, user);
            if (updateResult.IsFailure)
            {
                return ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData);
            }

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<ServiceResult<bool>> DecreaseUserStorageAsync(Guid userId, long removedBytes)
        {
            var userResult = await authHelper.GetUserByIdAsync(userId);
            if (userResult.IsFailure || userResult.Data == null)
            {
                return userResult.ToFailureResult<bool>();
            }
            
            var user = userResult.Data;

            var newUsage = user.UsedStorageBytes - removedBytes;
            if (newUsage < 0) newUsage = 0;

            user.UsedStorageBytes = newUsage;

            var updateDto = new UpdateUserDTO
            {
                UsedStorageBytes = user.UsedStorageBytes
            };

            var updateResult = await authHelper.UpdateUserAsync(updateDto, user);
            if (updateResult.IsFailure)
            {
                return ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData);
            }

            return ServiceResult<bool>.SuccessResult(true);
        }
    }
}