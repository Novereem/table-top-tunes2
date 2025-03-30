using System.Collections.Concurrent;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;

namespace TTT2.Tests.FakeData;

public class FakeAuthData : IAuthenticationData
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task<DataResult<User>> RegisterUserAsync(User user)
    {
        // Check for duplicates by Username or Email.
        if (_users.Values.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)
                                   || u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(DataResult<User>.AlreadyExists());
        }

        // Add the user to the in-memory store.
        _users[user.Id] = user;
        return Task.FromResult(DataResult<User>.Success(user));
    }

    public Task<DataResult<User>> GetUserByUsernameAsync(string username)
    {
        var user = _users.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return user != null 
            ? Task.FromResult(DataResult<User>.Success(user))
            : Task.FromResult(DataResult<User>.NotFound());
    }

    public Task<DataResult<User>> GetUserByEmailAsync(string email)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return user != null 
            ? Task.FromResult(DataResult<User>.Success(user))
            : Task.FromResult(DataResult<User>.NotFound());
    }

    public Task<DataResult<User>> GetUserByIdAsync(Guid userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return Task.FromResult(DataResult<User>.Success(user));
        }
        return Task.FromResult(DataResult<User>.NotFound());
    }

    public Task<DataResult<User>> UpdateUserAsync(User user)
    {
        // Check if the user exists first.
        if (_users.ContainsKey(user.Id))
        {
            _users[user.Id] = user;
            return Task.FromResult(DataResult<User>.Success(user));
        }
        return Task.FromResult(DataResult<User>.NotFound());
    }

    // Optional helper: clear the in-memory store between tests.
    public void Clear() => _users.Clear();
}