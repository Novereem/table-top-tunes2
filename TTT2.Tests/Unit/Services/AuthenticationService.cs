using System.Security.Claims;
using Moq;
using Shared.Enums;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using Shared.Models.Extensions;
using Shared.Statics;
using TTT2.Services;

namespace TTT2.Tests.Unit.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IAuthenticationServiceHelper> _authHelperMock;
    private readonly Mock<IUserClaimsService> _userClaimsServiceMock;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _authHelperMock = new Mock<IAuthenticationServiceHelper>();
        _userClaimsServiceMock = new Mock<IUserClaimsService>();
        _service = new AuthenticationService(_authHelperMock.Object, _userClaimsServiceMock.Object);
    }

    // Helper to create dummy DTOs and a User.
    private RegisterDTO CreateDummyRegisterDTO(string username = "testuser", string email = "test@example.com", string password = "password") =>
        new RegisterDTO { Username = username, Email = email, Password = password };

    private LoginDTO CreateDummyLoginDTO(string username = "testuser", string password = "password") =>
        new LoginDTO { Username = username, Password = password };

    private UpdateUserDTO CreateDummyUpdateUserDTO(string newPassword = "newpassword", string oldPassword = "password") =>
        new UpdateUserDTO { Username = "testuser", Email = "test@example.com", OldPassword = oldPassword, NewPassword = newPassword, UsedStorageBytes = 0, MaxStorageBytes = 1024 };

    private User CreateDummyUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            UsedStorageBytes = 0,
            MaxStorageBytes = 1024,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Helper to create a dummy ClaimsPrincipal containing the user's id.
    private ClaimsPrincipal CreateDummyClaimsPrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        return new ClaimsPrincipal(identity);
    }

    #region RegisterUserAsync Tests

    [Fact]
    public async Task RegisterUserAsync_ValidationFails_ReturnsFailure()
    {
        var registerDTO = CreateDummyRegisterDTO();
        var validationFailure = ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
        _authHelperMock.Setup(x => x.ValidateRegistrationAsync(registerDTO))
            .ReturnsAsync(validationFailure);

        var result = await _service.RegisterUserAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RegisterUserAsync_RegisterFails_ReturnsFailure()
    {
        var registerDTO = CreateDummyRegisterDTO();
        _authHelperMock.Setup(x => x.ValidateRegistrationAsync(registerDTO))
            .ReturnsAsync(ServiceResult<object>.SuccessResult());
        var registrationFailure = ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData);
        _authHelperMock.Setup(x => x.RegisterUserAsync(registerDTO))
            .ReturnsAsync(registrationFailure);

        var result = await _service.RegisterUserAsync(registerDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RegisterUserAsync_Success_ReturnsSuccessResponse()
    {
        var registerDTO = CreateDummyRegisterDTO();
        _authHelperMock.Setup(x => x.ValidateRegistrationAsync(registerDTO))
            .ReturnsAsync(ServiceResult<object>.SuccessResult());
        var dummyUser = CreateDummyUser();
        _authHelperMock.Setup(x => x.RegisterUserAsync(registerDTO))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        // Assume the extension method ToRegisterResponseDTO converts the user properly.
        var expectedResponse = dummyUser.ToRegisterResponseDTO();

        var result = await _service.RegisterUserAsync(registerDTO);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.Username, result.Data.Username);
    }

    #endregion

    #region LoginUserAsync Tests

    [Fact]
    public async Task LoginUserAsync_ValidationFails_ReturnsFailure()
    {
        var loginDTO = CreateDummyLoginDTO();
        var failureResult = ServiceResult<User>.Failure(MessageKey.Error_InvalidCredentials);
        _authHelperMock.Setup(x => x.ValidateLoginAsync(loginDTO))
            .ReturnsAsync(failureResult);

        var result = await _service.LoginUserAsync(loginDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidCredentials);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task LoginUserAsync_TokenGenerationFails_ReturnsFailure()
    {
        var loginDTO = CreateDummyLoginDTO();
        var dummyUser = CreateDummyUser();
        _authHelperMock.Setup(x => x.ValidateLoginAsync(loginDTO))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        var tokenFailure = ServiceResult<string>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _authHelperMock.Setup(x => x.GenerateJwtToken(dummyUser))
            .Returns(tokenFailure);

        var result = await _service.LoginUserAsync(loginDTO);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task LoginUserAsync_Success_ReturnsSuccessResponse()
    {
        var loginDTO = CreateDummyLoginDTO();
        var dummyUser = CreateDummyUser();
        _authHelperMock.Setup(x => x.ValidateLoginAsync(loginDTO))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        var tokenSuccess = ServiceResult<string>.SuccessResult("validtoken");
        _authHelperMock.Setup(x => x.GenerateJwtToken(dummyUser))
            .Returns(tokenSuccess);

        var result = await _service.LoginUserAsync(loginDTO);

        Assert.True(result.IsSuccess);
        Assert.Equal("validtoken", result.Data.Token);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failureUserIdResult = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failureUserIdResult);

        var updateDTO = CreateDummyUpdateUserDTO();
        var result = await _service.UpdateUserAsync(updateDTO, claims);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task UpdateUserAsync_UserRetrievalFails_ReturnsFailure2()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var failureUserResult = ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(failureUserResult);

        var updateDTO = CreateDummyUpdateUserDTO();
        var result = await _service.UpdateUserAsync(updateDTO, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateUserAsync_ValidationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        var validationFailure = ServiceResult<bool>.Failure(MessageKey.Error_InvalidOldPassword);
        _authHelperMock.Setup(x => x.ValidateUserUpdateAsync(It.IsAny<UpdateUserDTO>(), dummyUser))
            .ReturnsAsync(validationFailure);

        var updateDTO = CreateDummyUpdateUserDTO();
        var result = await _service.UpdateUserAsync(updateDTO, claims);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidOldPassword);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdateFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _authHelperMock.Setup(x => x.ValidateUserUpdateAsync(It.IsAny<UpdateUserDTO>(), dummyUser))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), dummyUser))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData));

        var updateDTO = CreateDummyUpdateUserDTO();
        var result = await _service.UpdateUserAsync(updateDTO, claims);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task UpdateUserAsync_Success_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var updateDTO = CreateDummyUpdateUserDTO();
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _authHelperMock.Setup(x => x.ValidateUserUpdateAsync(It.IsAny<UpdateUserDTO>(), dummyUser))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        var updatedUser = dummyUser; // For testing, assume update returns same user.
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), dummyUser))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(updatedUser));

        var result = await _service.UpdateUserAsync(updateDTO, claims);

        Assert.True(result.IsSuccess);
        Assert.Equal(updatedUser.Username, result.Data.Username);
        Assert.Equal(updatedUser.Email, result.Data.Email);
    }

    #endregion

    #region GetUserByIdAsync Test

    [Fact]
    public async Task GetUserByIdAsync_DelegatesToHelper_ReturnsResult()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var helperResult = ServiceResult<User>.SuccessResult(dummyUser);
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(helperResult);

        var result = await _service.GetUserByIdAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(dummyUser.Id, result.Data.Id);
    }

    #endregion
}