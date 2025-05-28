using Moq;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.Extensions;
using Shared.Statics;
using TTT2.Services.Helpers;

namespace TTT2.Tests.Unit.Services.Helpers;

[Trait("Category", "Unit")]
public class SceneAudioServiceHelperTests
{
    private readonly Mock<ISceneAudioData> _sceneAudioDataMock;
    private readonly SceneAudioServiceHelper _helper;

    public SceneAudioServiceHelperTests()
    {
        _sceneAudioDataMock = new Mock<ISceneAudioData>();
        _helper = new SceneAudioServiceHelper(_sceneAudioDataMock.Object);
    }

    // Helper: Create a dummy SceneAudioAssignDTO.
    private SceneAudioAssignDTO CreateDummySceneAudioAssignDTO()
    {
        return new SceneAudioAssignDTO
        {
            SceneId = Guid.NewGuid(),
            AudioFileId = Guid.NewGuid(),
            AudioType = AudioType.Music // or another valid enum value
        };
    }

    // Helper: Create a dummy SceneAudioRemoveDTO.
    private SceneAudioRemoveDTO CreateDummySceneAudioRemoveDTO()
    {
        return new SceneAudioRemoveDTO
        {
            SceneId = Guid.NewGuid(),
            AudioFileId = Guid.NewGuid(),
            AudioType = AudioType.Music
        };
    }

    // Helper: Create a dummy SceneAudioRemoveAllDTO.
    private SceneAudioRemoveAllDTO CreateDummySceneAudioRemoveAllDTO()
    {
        return new SceneAudioRemoveAllDTO
        {
            SceneId = Guid.NewGuid()
        };
    }

    // Helper: Create a dummy SceneAudioGetDTO.
    private SceneAudioGetDTO CreateDummySceneAudioGetDTO()
    {
        return new SceneAudioGetDTO
        {
            SceneId = Guid.NewGuid()
        };
    }

    // Helper: Create a dummy SceneAudioFile.
    private SceneAudioFile CreateDummySceneAudioFile()
    {
        return new SceneAudioFile
        {
            SceneId = Guid.NewGuid(),
            AudioFileId = Guid.NewGuid(),
            AudioType = AudioType.Music
        };
    }

    #region AddSceneAudioFileAsync Tests

    [Fact]
    public async Task AddSceneAudioFileAsync_DataError_ReturnsFailureInternalServerErrorData()
    {
        var dto = CreateDummySceneAudioAssignDTO();
        // When converting dto to a SceneAudioFile, the extension method is used.
        _sceneAudioDataMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ReturnsAsync(DataResult<SceneAudioFile>.Error());

        var result = await _helper.AddSceneAudioFileAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task AddSceneAudioFileAsync_AlreadyExists_ReturnsFailureSceneAudioAlreadyAdded()
    {
        var dto = CreateDummySceneAudioAssignDTO();
        _sceneAudioDataMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ReturnsAsync(DataResult<SceneAudioFile>.AlreadyExists());

        var result = await _helper.AddSceneAudioFileAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_SceneAudioAlreadyAdded);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task AddSceneAudioFileAsync_Success_ReturnsSuccessResponse()
    {
        var dto = CreateDummySceneAudioAssignDTO();
        var dummyFile = CreateDummySceneAudioFile();
        // Assume the extension method converts the file to a response DTO.
        var expectedResponse = dummyFile.ToSceneAudioAssignDTO();
        _sceneAudioDataMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ReturnsAsync(DataResult<SceneAudioFile>.Success(dummyFile));

        var result = await _helper.AddSceneAudioFileAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.SceneId, result.Data.SceneId);
        Assert.Equal(expectedResponse.AudioFileId, result.Data.AudioFileId);
        Assert.Equal(expectedResponse.AudioType, result.Data.AudioType);
    }

    [Fact]
    public async Task AddSceneAudioFileAsync_Exception_ReturnsFailureInternalServerErrorData()
    {
        var dto = CreateDummySceneAudioAssignDTO();
        _sceneAudioDataMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.AddSceneAudioFileAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region RemoveSceneAudioFileAsync Tests

    [Fact]
    public async Task RemoveSceneAudioFileAsync_Success_ReturnsSuccess()
    {
        var dto = CreateDummySceneAudioRemoveDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ReturnsAsync(DataResult<bool>.Success(true));

        var result = await _helper.RemoveSceneAudioFileAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task RemoveSceneAudioFileAsync_NotFound_ReturnsFailureNotFound()
    {
        var dto = CreateDummySceneAudioRemoveDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ReturnsAsync(DataResult<bool>.NotFound());

        var result = await _helper.RemoveSceneAudioFileAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RemoveSceneAudioFileAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var dto = CreateDummySceneAudioRemoveDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ReturnsAsync(DataResult<bool>.Error());

        var result = await _helper.RemoveSceneAudioFileAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RemoveSceneAudioFileAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var dto = CreateDummySceneAudioRemoveDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveSceneAudioFileAsync(It.IsAny<SceneAudioFile>()))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.RemoveSceneAudioFileAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region RemoveAllSceneAudioFilesAsync Tests

    [Fact]
    public async Task RemoveAllSceneAudioFilesAsync_Success_ReturnsSuccess()
    {
        var dto = CreateDummySceneAudioRemoveAllDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveAllSceneAudioFilesAsync(dto.SceneId))
            .ReturnsAsync(DataResult<bool>.Success(true));

        var result = await _helper.RemoveAllSceneAudioFilesAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task RemoveAllSceneAudioFilesAsync_NotFound_ReturnsFailureNotFound()
    {
        var dto = CreateDummySceneAudioRemoveAllDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveAllSceneAudioFilesAsync(dto.SceneId))
            .ReturnsAsync(DataResult<bool>.NotFound());

        var result = await _helper.RemoveAllSceneAudioFilesAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RemoveAllSceneAudioFilesAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var dto = CreateDummySceneAudioRemoveAllDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveAllSceneAudioFilesAsync(dto.SceneId))
            .ReturnsAsync(DataResult<bool>.Error());

        var result = await _helper.RemoveAllSceneAudioFilesAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task RemoveAllSceneAudioFilesAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var dto = CreateDummySceneAudioRemoveAllDTO();
        _sceneAudioDataMock.Setup(x => x.RemoveAllSceneAudioFilesAsync(dto.SceneId))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.RemoveAllSceneAudioFilesAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region GetSceneAudioFilesAsync Tests

    [Fact]
    public async Task GetSceneAudioFilesAsync_Success_ReturnsSceneAudioFiles()
    {
        var dto = CreateDummySceneAudioGetDTO();
        var dummyFiles = new List<SceneAudioFile> { CreateDummySceneAudioFile(), CreateDummySceneAudioFile() };
        _sceneAudioDataMock.Setup(x => x.GetSceneAudioFilesBySceneIdAsync(dto.SceneId))
            .ReturnsAsync(DataResult<List<SceneAudioFile>>.Success(dummyFiles));

        var result = await _helper.GetSceneAudioFilesAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(dummyFiles.Count, result.Data.Count);
    }

    [Fact]
    public async Task GetSceneAudioFilesAsync_NotFound_ReturnsEmptyList()
    {
        var dto = CreateDummySceneAudioGetDTO();
        _sceneAudioDataMock.Setup(x => x.GetSceneAudioFilesBySceneIdAsync(dto.SceneId))
            .ReturnsAsync(DataResult<List<SceneAudioFile>>.NotFound());

        var result = await _helper.GetSceneAudioFilesAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetSceneAudioFilesAsync_Error_ReturnsFailureInternalServerErrorData()
    {
        var dto = CreateDummySceneAudioGetDTO();
        _sceneAudioDataMock.Setup(x => x.GetSceneAudioFilesBySceneIdAsync(dto.SceneId))
            .ReturnsAsync(DataResult<List<SceneAudioFile>>.Error());

        var result = await _helper.GetSceneAudioFilesAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task GetSceneAudioFilesAsync_Exception_ReturnsFailureInternalServerErrorService()
    {
        var dto = CreateDummySceneAudioGetDTO();
        _sceneAudioDataMock.Setup(x => x.GetSceneAudioFilesBySceneIdAsync(dto.SceneId))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _helper.GetSceneAudioFilesAsync(dto);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion
}