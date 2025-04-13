using Moq;
using Shared.Enums;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using Shared.Statics;
using TTT2.Services;

namespace TTT2.Tests.Unit.Services;

public class UserStorageServiceTests
{
    private readonly Mock<IAuthenticationServiceHelper> _authHelperMock;
    private readonly UserStorageService _service;

    public UserStorageServiceTests()
    {
        _authHelperMock = new Mock<IAuthenticationServiceHelper>();
        _service = new UserStorageService(_authHelperMock.Object);
    }

    private User CreateDummyUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "dummyUser",
            Email = "dummy@example.com",
            PasswordHash = "hashed",
            UsedStorageBytes = 500,
            MaxStorageBytes = 1000,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task IncreaseUserStorageAsync_Success_ReturnsSuccess()
    {
        // Arrange
        var user = CreateDummyUser();
        long additionalBytes = 400; // New usage will be 900, within max (1000)
        _authHelperMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), user))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));

        // Act
        var result = await _service.IncreaseUserStorageAsync(user.Id, additionalBytes);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IncreaseUserStorageAsync_ExceedsQuota_ReturnsFailure()
    {
        // Arrange
        var user = CreateDummyUser();
        long additionalBytes = 600; // New usage = 500+600 = 1100, exceeds max 1000
        _authHelperMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));

        // Act
        var result = await _service.IncreaseUserStorageAsync(user.Id, additionalBytes);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_ExceedsStorageQuota);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task IncreaseUserStorageAsync_GetUserFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService));

        // Act
        var result = await _service.IncreaseUserStorageAsync(userId, 100);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task IncreaseUserStorageAsync_UpdateFails_ReturnsFailure()
    {
        // Arrange
        var user = CreateDummyUser();
        long additionalBytes = 100;
        _authHelperMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), user))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData));

        // Act
        var result = await _service.IncreaseUserStorageAsync(user.Id, additionalBytes);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task DecreaseUserStorageAsync_Success_ReturnsSuccess()
    {
        // Arrange
        var user = CreateDummyUser();
        long removedBytes = 200; // New usage = 500-200 = 300
        _authHelperMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), user))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));

        // Act
        var result = await _service.DecreaseUserStorageAsync(user.Id, removedBytes);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DecreaseUserStorageAsync_NegativeUsage_SetsToZero_ReturnsSuccess()
    {
        // Arrange
        var user = CreateDummyUser();
        long removedBytes = 600; // New usage = 500-600 = -100, should set to 0
        _authHelperMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), user))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));

        // Act
        var result = await _service.DecreaseUserStorageAsync(user.Id, removedBytes);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DecreaseUserStorageAsync_GetUserFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _authHelperMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService));

        // Act
        var result = await _service.DecreaseUserStorageAsync(userId, 100);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DecreaseUserStorageAsync_UpdateFails_ReturnsFailure()
    {
        // Arrange
        var user = CreateDummyUser();
        long removedBytes = 100;
        _authHelperMock.Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(user));
        _authHelperMock.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUserDTO>(), user))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorData));

        // Act
        var result = await _service.DecreaseUserStorageAsync(user.Id, removedBytes);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }
}