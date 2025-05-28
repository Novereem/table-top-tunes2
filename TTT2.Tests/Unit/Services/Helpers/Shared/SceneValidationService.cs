using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Models.Common;
using Shared.Statics;
using TTT2.Services.Helpers.Shared;
using Moq;

namespace TTT2.Tests.Unit.Services.Helpers.Shared;

[Trait("Category", "Unit")]
public class SceneValidationServiceTests
{
    private readonly Mock<ISceneData> _sceneDataMock;
    private readonly SceneValidationService _service;

    public SceneValidationServiceTests()
    {
        _sceneDataMock = new Mock<ISceneData>();
        _service = new SceneValidationService(_sceneDataMock.Object);
    }

    [Fact]
    public async Task ValidateSceneWithUserAsync_WhenSceneBelongsToUser_ReturnsSuccess()
    {
        // Arrange
        Guid sceneId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var dataResult = DataResult<bool>.Success(true);
        _sceneDataMock
            .Setup(x => x.SceneBelongsToUserAsync(sceneId, userId))
            .ReturnsAsync(dataResult);

        // Act
        var result = await _service.ValidateSceneWithUserAsync(sceneId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateSceneWithUserAsync_WhenSceneNotFound_ReturnsFailureUnauthorized()
    {
        // Arrange
        Guid sceneId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var dataResult = DataResult<bool>.NotFound();
        _sceneDataMock
            .Setup(x => x.SceneBelongsToUserAsync(sceneId, userId))
            .ReturnsAsync(dataResult);

        // Act
        var result = await _service.ValidateSceneWithUserAsync(sceneId, userId);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_Unauthorized);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public async Task ValidateSceneWithUserAsync_WhenDataError_ReturnsFailureInternalServerErrorData()
    {
        // Arrange
        Guid sceneId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var dataResult = DataResult<bool>.Error();
        _sceneDataMock
            .Setup(x => x.SceneBelongsToUserAsync(sceneId, userId))
            .ReturnsAsync(dataResult);

        // Act
        var result = await _service.ValidateSceneWithUserAsync(sceneId, userId);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public async Task ValidateSceneWithUserAsync_WhenExceptionThrown_ReturnsFailureInternalServerErrorService()
    {
        // Arrange
        Guid sceneId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        _sceneDataMock
            .Setup(x => x.SceneBelongsToUserAsync(sceneId, userId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _service.ValidateSceneWithUserAsync(sceneId, userId);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }
}