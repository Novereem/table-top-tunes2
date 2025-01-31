using Shared.Models.Common;

namespace Shared.Interfaces.Services
{
    public interface IUserStorageService
    {
        Task<ServiceResult<bool>> IncreaseUserStorageAsync(Guid userId, long additionalBytes);
        Task<ServiceResult<bool>> DecreaseUserStorageAsync(Guid userId, long removedBytes);
    }
}