using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Common;

namespace Shared.Interfaces.Data
{
    public interface IAuthenticationData
    {
        Task<DataResult<User>> RegisterUserAsync(User user);
        Task<DataResult<User>> GetUserByUsernameAsync(string username);
        Task<DataResult<User>> GetUserByEmailAsync(string email);
        Task<DataResult<User>> GetUserByIdAsync(Guid userId);
    }
}
