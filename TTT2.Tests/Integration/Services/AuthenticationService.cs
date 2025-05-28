using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using TTT2.Services;

[Trait("Category", "Integration")]
public class FakeAuthenticationServiceHelper : IAuthenticationServiceHelper
{
    public Task<ServiceResult<object>> ValidateRegistrationAsync(RegisterDTO registerDTO)
    {
        return Task.FromResult(ServiceResult<object>.SuccessResult());
    }

    public Task<ServiceResult<User>> RegisterUserAsync(RegisterDTO registerDTO)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = registerDTO.Username,
            Email = registerDTO.Email,
            PasswordHash = "hashedpassword",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<User>.SuccessResult(user));
    }

    public Task<ServiceResult<User>> ValidateLoginAsync(LoginDTO loginDTO)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = loginDTO.Username,
            Email = "dummy@example.com",
            PasswordHash = "hashedpassword",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<User>.SuccessResult(user));
    }

    public ServiceResult<string> GenerateJwtToken(User user)
    {
        // Return a fake JWT token.
        return ServiceResult<string>.SuccessResult("fake.jwt.token");
    }

    public Task<ServiceResult<bool>> ValidateUserUpdateAsync(UpdateUserDTO updateUserDTO, User user)
    {
        return Task.FromResult(ServiceResult<bool>.SuccessResult());
    }

    public Task<ServiceResult<User>> UpdateUserAsync(UpdateUserDTO updateUserDTO, User user)
    {
        if (!string.IsNullOrWhiteSpace(updateUserDTO.Username))
            user.Username = updateUserDTO.Username;
        if (!string.IsNullOrWhiteSpace(updateUserDTO.Email))
            user.Email = updateUserDTO.Email;
        if (updateUserDTO.UsedStorageBytes.HasValue)
            user.UsedStorageBytes = updateUserDTO.UsedStorageBytes.Value;
        if (updateUserDTO.MaxStorageBytes.HasValue)
            user.MaxStorageBytes = updateUserDTO.MaxStorageBytes.Value;

        return Task.FromResult(ServiceResult<User>.SuccessResult(user));
    }

    public Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
    {
        var user = new User
        {
            Id = userId,
            Username = "TestUser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<User>.SuccessResult(user));
    }
}

public class FakeUserClaimsServiceAuthenticationService : IUserClaimsService
{
    public ServiceResult<Guid> GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return ServiceResult<Guid>.Failure(MessageKey.Error_InvalidInput);
        return ServiceResult<Guid>.SuccessResult(userId);
    }
}

namespace TTT2.Tests.IntegrationTests
{
    public class AuthenticationServiceIntegrationTests
    {
        private readonly IAuthenticationServiceHelper _authHelper = new FakeAuthenticationServiceHelper();
        private readonly AuthenticationService _authService;
        private readonly ClaimsPrincipal _testUser;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly IUserClaimsService _userClaimsService = new FakeUserClaimsServiceAuthenticationService();

        public AuthenticationServiceIntegrationTests()
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()));
            _testUser = new ClaimsPrincipal(identity);

            _authService = new AuthenticationService(_authHelper, _userClaimsService);
        }

        [Fact]
        public async Task RegisterUserAsync_Successful_Test()
        {
            // Arrange: create a RegisterDTO with valid data.
            var registerDTO = new RegisterDTO
            {
                Username = "NewUser",
                Email = "newuser@example.com",
                Password = "SecurePassword123"
            };

            // Act: Call RegisterUserAsync.
            var result = await _authService.RegisterUserAsync(registerDTO);

            // Assert: Ensure that the registration succeeds and the returned DTO contains expected data.
            Assert.True(result.IsSuccess, "User registration should succeed.");
            Assert.NotNull(result.Data);
            Assert.Equal("NewUser", result.Data.Username);
        }

        [Fact]
        public async Task LoginUserAsync_Successful_Test()
        {
            // Arrange: create a LoginDTO with valid data.
            var loginDTO = new LoginDTO
            {
                Username = "ExistingUser",
                Password = "ValidPassword"
            };

            // Act: Call LoginUserAsync.
            var result = await _authService.LoginUserAsync(loginDTO);

            // Assert: Ensure that login succeeds and a token is returned.
            Assert.True(result.IsSuccess, "User login should succeed.");
            Assert.NotNull(result.Data);
            Assert.Equal("fake.jwt.token", result.Data.Token);
        }

        [Fact]
        public async Task UpdateUserAsync_Successful_Test()
        {
            // Arrange: create an UpdateUserDTO with changes.
            var updateDTO = new UpdateUserDTO
            {
                Username = "UpdatedUser",
                Email = "updateduser@example.com",
                UsedStorageBytes = 5000,
                MaxStorageBytes = 20 * 1024 * 1024
            };

            // Act: Call UpdateUserAsync.
            var result = await _authService.UpdateUserAsync(updateDTO, _testUser);

            // Assert: Ensure the update succeeds and the returned DTO reflects the changes.
            Assert.True(result.IsSuccess, "User update should succeed.");
            Assert.NotNull(result.Data);
            Assert.Equal("UpdatedUser", result.Data.Username);
            Assert.Equal("updateduser@example.com", result.Data.Email);
            Assert.Equal(5000, result.Data.UsedStorageBytes);
            Assert.Equal(20 * 1024 * 1024, result.Data.MaxStorageBytes);
        }
    }
}