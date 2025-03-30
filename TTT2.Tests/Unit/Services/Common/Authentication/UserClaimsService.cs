using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Models.Common;
using Shared.Statics;
using TTT2.Services.Common.Authentication;

namespace TTT2.Tests.Unit.Services.Common.Authentication;

public class UserClaimsServiceTests
{
    private readonly IUserClaimsService _userClaimsService = new UserClaimsService();

    [Fact]
    public void GetUserIdFromClaims_NoNameIdentifierClaim_ReturnsFailureWithJWTNullOrEmptyMessage()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // No claims added.
        var user = new ClaimsPrincipal(identity);

        // Act
        ServiceResult<Guid> result = _userClaimsService.GetUserIdFromClaims(user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void GetUserIdFromClaims_InvalidGuidClaim_ReturnsFailureWithUnauthorizedMessage()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "not-a-valid-guid"));
        var user = new ClaimsPrincipal(identity);

        // Act
        ServiceResult<Guid> result = _userClaimsService.GetUserIdFromClaims(user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_Unauthorized);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void GetUserIdFromClaims_ValidGuidClaim_ReturnsSuccessWithCorrectUserId()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()));
        var user = new ClaimsPrincipal(identity);

        // Act
        ServiceResult<Guid> result = _userClaimsService.GetUserIdFromClaims(user);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUserId, result.Data);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Success_OperationCompleted);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }
}