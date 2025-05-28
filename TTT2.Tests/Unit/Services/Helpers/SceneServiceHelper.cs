using Moq;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using Shared.Statics;
using TTT2.Services.Helpers;

namespace TTT2.Tests.Unit.Services.Helpers;

[Trait("Category", "Unit")]
public class SceneServiceHelperTests
{
    private readonly Mock<ISceneData> _sceneDataMock;
    private readonly Mock<ISceneValidationService> _sceneValidationServiceMock;
    private readonly SceneServiceHelper _helper;

    public SceneServiceHelperTests()
    {
        _sceneDataMock = new Mock<ISceneData>();
        _sceneValidationServiceMock = new Mock<ISceneValidationService>();
        _helper = new SceneServiceHelper(_sceneDataMock.Object, _sceneValidationServiceMock.Object);
    }

    // Helpers for DTOs and models
    private SceneCreateDTO CreateDummySceneCreateDTO(string name = "Test Scene")
    {
        return new SceneCreateDTO { Name = name };
    }

    private SceneGetDTO CreateDummySceneGetDTO(Guid sceneId) => new SceneGetDTO { SceneId = sceneId };

    private SceneRemoveDTO CreateDummySceneRemoveDTO(Guid sceneId) => new SceneRemoveDTO { SceneId = sceneId };

    private SceneUpdateDTO CreateDummySceneUpdateDTO(Guid sceneId, string newName = "Updated Scene")
    {
        return new SceneUpdateDTO { SceneId = sceneId, NewName = newName };
    }

    private User CreateDummyUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Scene CreateDummyScene(Guid id, string name, Guid userId)
    {
        return new Scene
        {
            Id = id,
            Name = name,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region ValidateSceneCreate Tests

    [Fact]
    public void ValidateSceneCreate_InvalidInput_ReturnsFailure()
    {
        var dto = CreateDummySceneCreateDTO(name: "  ");
        var result = _helper.ValidateSceneCreate(dto);
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void ValidateSceneCreate_ValidInput_ReturnsSuccess()
    {
        var dto = CreateDummySceneCreateDTO("My Scene");
        var result = _helper.ValidateSceneCreate(dto);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region CreateSceneAsync Tests

    [Fact]
    public async Task CreateSceneAsync_Success_ReturnsSuccessResponse()
    {
        var dto = CreateDummySceneCreateDTO("New Scene");
        var user = CreateDummyUser();
        var newScene = CreateDummyScene(Guid.NewGuid(), dto.Name, user.Id);

        _sceneDataMock.Setup(x => x.CreateSceneAsync(It.IsAny<Scene>()))
            .ReturnsAsync(DataResult<Scene>.Success(newScene));

        // Act
        var result = await _helper.CreateSceneAsync(dto, user);

        // Assert
        Assert.True(result.IsSuccess);
        // The service helper transforms 'newScene' -> SceneCreateResponseDTO
        Assert.Equal(newScene.Id, result.Data.Id);
        Assert.Equal(newScene.Name, result.Data.Name);
    }

    [Fact]
    public async Task CreateSceneAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var dto = CreateDummySceneCreateDTO("New Scene");
        var user = CreateDummyUser();

        _sceneDataMock.Setup(x => x.CreateSceneAsync(It.IsAny<Scene>()))
            .ReturnsAsync(DataResult<Scene>.Error());

        var result = await _helper.CreateSceneAsync(dto, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateSceneAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var dto = CreateDummySceneCreateDTO("New Scene");
        var user = CreateDummyUser();

        _sceneDataMock.Setup(x => x.CreateSceneAsync(It.IsAny<Scene>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.CreateSceneAsync(dto, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region RetrieveSceneBySceneIdAsync Tests

    [Fact]
    public async Task RetrieveSceneBySceneIdAsync_Success_ReturnsSceneGetResponseDTO()
    {
        var sceneId = Guid.NewGuid();
        var dto = CreateDummySceneGetDTO(sceneId);

        // The data layer returns a Scene
        var retrievedScene = CreateDummyScene(sceneId, "Retrieved Scene", Guid.NewGuid());
        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ReturnsAsync(DataResult<Scene>.Success(retrievedScene));

        var result = await _helper.RetrieveSceneBySceneIdAsync(dto);

        Assert.True(result.IsSuccess);
        // The helper transforms the retrieved Scene into SceneGetResponseDTO
        Assert.Equal(retrievedScene.Id, result.Data.Id);
        Assert.Equal(retrievedScene.Name, result.Data.Name);
    }

    [Fact]
    public async Task RetrieveSceneBySceneIdAsync_NotFound_ReturnsFailureNotFound()
    {
        var sceneId = Guid.NewGuid();
        var dto = CreateDummySceneGetDTO(sceneId);

        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ReturnsAsync(DataResult<Scene>.NotFound());

        var result = await _helper.RetrieveSceneBySceneIdAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RetrieveSceneBySceneIdAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var sceneId = Guid.NewGuid();
        var dto = CreateDummySceneGetDTO(sceneId);

        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ReturnsAsync(DataResult<Scene>.Error());

        var result = await _helper.RetrieveSceneBySceneIdAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RetrieveSceneBySceneIdAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var sceneId = Guid.NewGuid();
        var dto = CreateDummySceneGetDTO(sceneId);

        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.RetrieveSceneBySceneIdAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region RetrieveScenesByUserIdAsync Tests

    [Fact]
    public async Task RetrieveScenesByUserIdAsync_Success_ReturnsSceneList()
    {
        var user = CreateDummyUser();
        var scenes = new List<Scene>
        {
            new Scene { Id = Guid.NewGuid(), Name = "Scene 1", UserId = user.Id, CreatedAt = DateTime.UtcNow },
            new Scene { Id = Guid.NewGuid(), Name = "Scene 2", UserId = user.Id, CreatedAt = DateTime.UtcNow }
        };

        _sceneDataMock.Setup(x => x.GetScenesByUserIdAsync(user.Id))
            .ReturnsAsync(DataResult<List<Scene>>.Success(scenes));

        var result = await _helper.RetrieveScenesByUserIdAsync(user);

        Assert.True(result.IsSuccess);
        Assert.Equal(scenes.Count, result.Data.Count);
    }

    [Fact]
    public async Task RetrieveScenesByUserIdAsync_NotFound_ReturnsEmptyList()
    {
        var user = CreateDummyUser();

        _sceneDataMock.Setup(x => x.GetScenesByUserIdAsync(user.Id))
            .ReturnsAsync(DataResult<List<Scene>>.NotFound());

        var result = await _helper.RetrieveScenesByUserIdAsync(user);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task RetrieveScenesByUserIdAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var user = CreateDummyUser();

        _sceneDataMock.Setup(x => x.GetScenesByUserIdAsync(user.Id))
            .ReturnsAsync(DataResult<List<Scene>>.Error());

        var result = await _helper.RetrieveScenesByUserIdAsync(user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RetrieveScenesByUserIdAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var user = CreateDummyUser();

        _sceneDataMock.Setup(x => x.GetScenesByUserIdAsync(user.Id))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.RetrieveScenesByUserIdAsync(user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region ValidateSceneUpdate Tests

    [Fact]
    public void ValidateSceneUpdate_InvalidInput_ReturnsFailure()
    {
        var dto = new SceneUpdateDTO { SceneId = Guid.Empty, NewName = "  " };
        var result = _helper.ValidateSceneUpdate(dto);
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void ValidateSceneUpdate_ValidInput_ReturnsSuccess()
    {
        var dto = CreateDummySceneUpdateDTO(Guid.NewGuid(), "Updated Name");
        var result = _helper.ValidateSceneUpdate(dto);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region UpdateSceneAsync Tests

    [Fact]
    public async Task UpdateSceneAsync_ValidUpdate_ReturnsSuccessResponse()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var updateDTO = CreateDummySceneUpdateDTO(sceneId, "New Scene Name");

        // Set up scene validation to succeed.
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(sceneId, user.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        // We retrieve the scene from the DB as a Scene object
        var existingScene = CreateDummyScene(sceneId, "Old Scene Name", user.Id);
        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ReturnsAsync(DataResult<Scene>.Success(existingScene));

        // Then we update the scene in the DB
        var updatedScene = CreateDummyScene(sceneId, updateDTO.NewName, user.Id);
        _sceneDataMock.Setup(x => x.UpdateSceneAsync(It.IsAny<Scene>()))
            .ReturnsAsync(DataResult<Scene>.Success(updatedScene));

        // Act
        var result = await _helper.UpdateSceneAsync(updateDTO, user);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(updateDTO.NewName, result.Data.UpdatedName);
    }

    [Fact]
    public async Task UpdateSceneAsync_ValidationFails_ReturnsFailure()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var updateDTO = CreateDummySceneUpdateDTO(sceneId, "New Scene Name");

        // Simulate scene validation failure.
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(sceneId, user.Id))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized));

        var result = await _helper.UpdateSceneAsync(updateDTO, user);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateSceneAsync_RetrieveSceneFails_ReturnsFailure()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var updateDTO = CreateDummySceneUpdateDTO(sceneId, "New Scene Name");

        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(sceneId, user.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        // Simulate retrieval failure.
        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ReturnsAsync(DataResult<Scene>.NotFound());

        var result = await _helper.UpdateSceneAsync(updateDTO, user);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateSceneAsync_UpdateFails_ReturnsFailureInternalServerErrorData()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var updateDTO = CreateDummySceneUpdateDTO(sceneId, "New Scene Name");

        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(sceneId, user.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var existingScene = CreateDummyScene(sceneId, "Old Scene Name", user.Id);
        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ReturnsAsync(DataResult<Scene>.Success(existingScene));

        _sceneDataMock.Setup(x => x.UpdateSceneAsync(It.IsAny<Scene>()))
            .ReturnsAsync(DataResult<Scene>.Error());

        var result = await _helper.UpdateSceneAsync(updateDTO, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task UpdateSceneAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var updateDTO = CreateDummySceneUpdateDTO(sceneId, "New Scene Name");

        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(sceneId, user.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _sceneDataMock.Setup(x => x.GetSceneBySceneIdAsync(sceneId))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.UpdateSceneAsync(updateDTO, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region DeleteSceneAsync Tests

    [Fact]
    public async Task DeleteSceneAsync_Success_ReturnsSuccess()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var removeDTO = new SceneRemoveDTO { SceneId = sceneId };

        _sceneDataMock.Setup(x => x.DeleteSceneAsync(sceneId, user.Id))
            .ReturnsAsync(DataResult<bool>.Success(true));

        var result = await _helper.DeleteSceneAsync(removeDTO, user);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteSceneAsync_NotFound_ReturnsFailureNotFound()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var removeDTO = new SceneRemoveDTO { SceneId = sceneId };

        _sceneDataMock.Setup(x => x.DeleteSceneAsync(sceneId, user.Id))
            .ReturnsAsync(DataResult<bool>.NotFound());

        var result = await _helper.DeleteSceneAsync(removeDTO, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task DeleteSceneAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var removeDTO = new SceneRemoveDTO { SceneId = sceneId };

        _sceneDataMock.Setup(x => x.DeleteSceneAsync(sceneId, user.Id))
            .ReturnsAsync(DataResult<bool>.Error());

        var result = await _helper.DeleteSceneAsync(removeDTO, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task DeleteSceneAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var user = CreateDummyUser();
        var sceneId = Guid.NewGuid();
        var removeDTO = new SceneRemoveDTO { SceneId = sceneId };

        _sceneDataMock.Setup(x => x.DeleteSceneAsync(sceneId, user.Id))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.DeleteSceneAsync(removeDTO, user);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion
}