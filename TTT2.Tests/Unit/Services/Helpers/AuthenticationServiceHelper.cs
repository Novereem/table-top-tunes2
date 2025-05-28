using Moq;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using Shared.Statics;
using TTT2.Services.Helpers;

namespace TTT2.Tests.Unit.Services.Helpers;

[Trait("Category", "Unit")]
public class AuthenticationServiceHelperTests
{
    private readonly Mock<IAuthenticationData> _authDataMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly AuthenticationServiceHelper _helper;

    public AuthenticationServiceHelperTests()
    {
        _authDataMock = new Mock<IAuthenticationData>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        _helper = new AuthenticationServiceHelper(_authDataMock.Object, _passwordHashingServiceMock.Object);
    }

    // Helpers to create dummy DTOs and a User.
    private RegisterDTO CreateDummyRegisterDTO(string username = "testuser", string email = "test@example.com", string password = "password")
    {
        return new RegisterDTO
        {
            Username = username,
            Email = email,
            Password = password
        };
    }

    private LoginDTO CreateDummyLoginDTO(string username = "testuser", string password = "password")
    {
        return new LoginDTO
        {
            Username = username,
            Password = password
        };
    }

    private UpdateUserDTO CreateDummyUpdateUserDTO(string username = "newuser", string email = "new@example.com", string oldPassword = "password", string newPassword = "newpassword")
    {
        return new UpdateUserDTO
        {
            OldPassword = oldPassword,
            Username = username,
            Email = email,
            NewPassword = newPassword
        };
    }

    private User CreateDummyUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            UsedStorageBytes = 0,
            MaxStorageBytes = 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region ValidateRegistrationAsync Tests

    [Fact]
    public async Task ValidateRegistrationAsync_EmailAlreadyExists_ReturnsFailureEmailTaken()
    {
        var registerDTO = CreateDummyRegisterDTO();
        _authDataMock.Setup(x => x.GetUserByEmailAsync(registerDTO.Email))
            .ReturnsAsync(DataResult<User>.Success(CreateDummyUser()));

        var result = await _helper.ValidateRegistrationAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_EmailTaken);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_UsernameAlreadyExists_ReturnsFailureUsernameTaken()
    {
        var registerDTO = CreateDummyRegisterDTO();
        _authDataMock.Setup(x => x.GetUserByEmailAsync(registerDTO.Email))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(registerDTO.Username))
            .ReturnsAsync(DataResult<User>.Success(CreateDummyUser()));

        var result = await _helper.ValidateRegistrationAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_UsernameTaken);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_InvalidInput_ReturnsFailureInvalidInput()
    {
        var registerDTO = CreateDummyRegisterDTO(username: "");
        _authDataMock.Setup(x => x.GetUserByEmailAsync(registerDTO.Email))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(registerDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.ValidateRegistrationAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_PasswordTooShort_ReturnsFailurePasswordTooShort()
    {
        var registerDTO = CreateDummyRegisterDTO(password: "1234");
        _authDataMock.Setup(x => x.GetUserByEmailAsync(registerDTO.Email))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(registerDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.ValidateRegistrationAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_PasswordTooShort);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_InvalidEmailFormat_ReturnsFailureInternalServerErrorService()
    {
        var registerDTO = CreateDummyRegisterDTO(email: "invalid-email");
        _authDataMock.Setup(x => x.GetUserByEmailAsync(registerDTO.Email))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(registerDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.ValidateRegistrationAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_ValidInput_ReturnsSuccess()
    {
        var registerDTO = CreateDummyRegisterDTO();
        _authDataMock.Setup(x => x.GetUserByEmailAsync(registerDTO.Email))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(registerDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.ValidateRegistrationAsync(registerDTO);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region RegisterUserAsync Tests

    [Fact]
    public async Task RegisterUserAsync_Success_ReturnsUser()
    {
        var registerDTO = CreateDummyRegisterDTO();
        // Simulate conversion via extension method; assume it creates a User.
        var userFromDTO = CreateDummyUser();
        _authDataMock.Setup(x => x.RegisterUserAsync(It.IsAny<User>()))
            .ReturnsAsync(DataResult<User>.Success(userFromDTO));

        var result = await _helper.RegisterUserAsync(registerDTO);

        Assert.True(result.IsSuccess);
        Assert.Equal(userFromDTO.Id, result.Data.Id);
    }

    [Fact]
    public async Task RegisterUserAsync_Error_ReturnsFailure()
    {
        var registerDTO = CreateDummyRegisterDTO();
        _authDataMock.Setup(x => x.RegisterUserAsync(It.IsAny<User>()))
            .ReturnsAsync(DataResult<User>.Error());

        var result = await _helper.RegisterUserAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region ValidateLoginAsync Tests

    [Fact]
    public async Task ValidateLoginAsync_InvalidUsername_ReturnsFailureInvalidCredentials()
    {
        var loginDTO = CreateDummyLoginDTO();
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(loginDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.ValidateLoginAsync(loginDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidCredentials);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateLoginAsync_InvalidPassword_ReturnsFailureInvalidCredentials()
    {
        var loginDTO = CreateDummyLoginDTO();
        var user = CreateDummyUser();
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(loginDTO.Username))
            .ReturnsAsync(DataResult<User>.Success(user));
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(loginDTO.Password, user.PasswordHash))
            .Returns(false);

        var result = await _helper.ValidateLoginAsync(loginDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidCredentials);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateLoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var loginDTO = CreateDummyLoginDTO();
        var user = CreateDummyUser();
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(loginDTO.Username))
            .ReturnsAsync(DataResult<User>.Success(user));
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(loginDTO.Password, user.PasswordHash))
            .Returns(true);

        var result = await _helper.ValidateLoginAsync(loginDTO);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Data.Id);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_Success_ReturnsUser()
    {
        var user = CreateDummyUser();
        _authDataMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(DataResult<User>.Success(user));

        var result = await _helper.GetUserByIdAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Data.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_NotFound_ReturnsFailureUnauthorized()
    {
        var userId = Guid.NewGuid();
        _authDataMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.GetUserByIdAsync(userId);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_Unauthorized);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task GetUserByIdAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var user = CreateDummyUser();
        _authDataMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(DataResult<User>.Error());

        var result = await _helper.GetUserByIdAsync(user.Id);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task GetUserByIdAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var userId = Guid.NewGuid();
        _authDataMock.Setup(x => x.GetUserByIdAsync(userId))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.GetUserByIdAsync(userId);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region ValidateUserUpdateAsync Tests

    [Fact]
    public async Task ValidateUserUpdateAsync_UsernameTaken_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO(username: "existingUser");
        var currentUser = CreateDummyUser();
        var otherUser = CreateDummyUser();
        otherUser.Id = Guid.NewGuid();
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(updateDTO.Username))
            .ReturnsAsync(DataResult<User>.Success(otherUser));

        var result = await _helper.ValidateUserUpdateAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_UsernameTaken);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateUserUpdateAsync_InvalidEmailFormat_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO(email: "invalid-email");
        var currentUser = CreateDummyUser();
        
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(DataResult<User>.NotFound());
    
        var result = await _helper.ValidateUserUpdateAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidEmail);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }


    [Fact]
    public async Task ValidateUserUpdateAsync_EmailTaken_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO(email: "new@example.com");
        var currentUser = CreateDummyUser();
        
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(updateDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());
        
        var otherUser = CreateDummyUser();
        otherUser.Id = Guid.NewGuid();
        _authDataMock.Setup(x => x.GetUserByEmailAsync(updateDTO.Email))
            .ReturnsAsync(DataResult<User>.Success(otherUser));

        var result = await _helper.ValidateUserUpdateAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_EmailTaken);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateUserUpdateAsync_InvalidOldPassword_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO(oldPassword: "wrongpassword", newPassword: "newpass");
        var currentUser = CreateDummyUser();
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(DataResult<User>.NotFound());
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(updateDTO.OldPassword, currentUser.PasswordHash))
            .Returns(false);

        var result = await _helper.ValidateUserUpdateAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidOldPassword);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateUserUpdateAsync_NewPasswordTooShort_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO(newPassword: "1234", oldPassword: "password");
        var currentUser = CreateDummyUser();
        
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(DataResult<User>.NotFound());
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(updateDTO.OldPassword, currentUser.PasswordHash))
            .Returns(true);

        var result = await _helper.ValidateUserUpdateAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_PasswordTooShort);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateUserUpdateAsync_AllValid_ReturnsSuccess()
    {
        var updateDTO = CreateDummyUpdateUserDTO();
        var currentUser = CreateDummyUser();
        _authDataMock.Setup(x => x.GetUserByUsernameAsync(updateDTO.Username))
            .ReturnsAsync(DataResult<User>.NotFound());
        _authDataMock.Setup(x => x.GetUserByEmailAsync(updateDTO.Email))
            .ReturnsAsync(DataResult<User>.NotFound());
        _passwordHashingServiceMock.Setup(x => x.VerifyPassword(updateDTO.OldPassword, currentUser.PasswordHash))
            .Returns(true);

        var result = await _helper.ValidateUserUpdateAsync(updateDTO, currentUser);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_Success_ReturnsUpdatedUser()
    {
        var updateDTO = CreateDummyUpdateUserDTO(username: "updatedUser", email: "updated@example.com", newPassword: "newpassword");
        var currentUser = CreateDummyUser();
        _passwordHashingServiceMock.Setup(x => x.HashPassword(updateDTO.NewPassword))
            .Returns("newhashedpassword");

        var updatedUser = currentUser;
        updatedUser.Username = updateDTO.Username;
        updatedUser.Email = updateDTO.Email;
        updatedUser.PasswordHash = "newhashedpassword";
        _authDataMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(DataResult<User>.Success(updatedUser));

        var result = await _helper.UpdateUserAsync(updateDTO, currentUser);

        Assert.True(result.IsSuccess);
        Assert.Equal(updateDTO.Username, result.Data.Username);
        Assert.Equal(updateDTO.Email, result.Data.Email);
        Assert.Equal("newhashedpassword", result.Data.PasswordHash);
    }

    [Fact]
    public async Task UpdateUserAsync_NotFound_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO();
        var currentUser = CreateDummyUser();
        _authDataMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(DataResult<User>.NotFound());

        var result = await _helper.UpdateUserAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task UpdateUserAsync_Error_ReturnsFailure()
    {
        var updateDTO = CreateDummyUpdateUserDTO();
        var currentUser = CreateDummyUser();
        _authDataMock.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(DataResult<User>.Error());

        var result = await _helper.UpdateUserAsync(updateDTO, currentUser);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region GenerateJwtToken Tests

    [Fact]
    public void GenerateJwtToken_NoSecretKey_ReturnsFailure()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);

        var result = _helper.GenerateJwtToken(CreateDummyUser());

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void GenerateJwtToken_WithSecretKey_ReturnsToken()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "supersecretkey123thatneedstobealittlelonger");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "TestIssuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "TestAudience");

        var result = _helper.GenerateJwtToken(CreateDummyUser());

        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        Environment.SetEnvironmentVariable("JWT_ISSUER", null);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", null);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Data));
    }

    #endregion
}