using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Enums;
using Shared.Interfaces.Data;
using Shared.Interfaces.Services.Helpers.FileValidation;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Statics;
using TTT2.Services.Helpers;

namespace TTT2.Tests.Unit.Services.Helpers;


public class AudioServiceHelperTests : IDisposable
{
    private readonly Mock<IAudioData> _audioDataMock;
    private readonly Mock<IAudioFileValidator> _audioFileValidatorMock;
    private readonly Mock<IFileSafetyValidator> _fileSafetyValidatorMock;
    private readonly AudioServiceHelper _helper;
    private readonly string _uploadsFolderPath;

    public AudioServiceHelperTests()
    {
        _audioDataMock = new Mock<IAudioData>();
        _audioFileValidatorMock = new Mock<IAudioFileValidator>();
        _fileSafetyValidatorMock = new Mock<IFileSafetyValidator>();

        _helper = new AudioServiceHelper(
            _audioDataMock.Object, 
            _audioFileValidatorMock.Object, 
            _fileSafetyValidatorMock.Object);

        _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
    }

    public void Dispose()
    {
        // Cleanup the Uploads folder created during tests.
        if (Directory.Exists(_uploadsFolderPath))
        {
            Directory.Delete(_uploadsFolderPath, true);
        }
    }

    // Helper: creates a dummy IFormFile.
    private IFormFile CreateTestFormFile(byte[] content, string fileName)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName);
    }

    // Helper: creates a dummy AudioFileCreateDTO.
    private AudioFileCreateDTO CreateDummyAudioFileCreateDTO(string fileName, string name, byte[] content)
    {
        return new AudioFileCreateDTO
        {
            AudioFile = CreateTestFormFile(content, fileName),
            Name = name
        };
    }

    // Helper: creates a fully populated dummy User.
    private User CreateDummyUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = "dummyUser",
            Email = "dummy@example.com",
            PasswordHash = "dummyHash",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region ValidateAudioFileCreateRequest Tests

    [Fact]
    public void ValidateAudioFileCreateRequest_BasicCheckFails_ReturnsFailure()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", new byte[1024]);
        var user = CreateDummyUser();

        // Simulate basic validation failure.
        var failureResult = ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
        _audioFileValidatorMock
            .Setup(x => x.ValidateFileBasics(dto, user))
            .Returns(failureResult);

        // Act
        var result = _helper.ValidateAudioFileCreateRequest(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void ValidateAudioFileCreateRequest_MagicCheckFails_ReturnsFailure()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", new byte[1024]);
        var user = CreateDummyUser();

        // Basic check passes.
        _audioFileValidatorMock
            .Setup(x => x.ValidateFileBasics(dto, user))
            .Returns(ServiceResult<object>.SuccessResult());
        // Magic check fails.
        var magicFailure = ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
        _audioFileValidatorMock
            .Setup(x => x.ValidateMagicNumber(dto.AudioFile))
            .Returns(magicFailure);

        // Act
        var result = _helper.ValidateAudioFileCreateRequest(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidAudioFileType);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void ValidateAudioFileCreateRequest_DecodeCheckFails_ReturnsFailure()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", new byte[1024]);
        var user = CreateDummyUser();

        // Basic and magic checks pass.
        _audioFileValidatorMock
            .Setup(x => x.ValidateFileBasics(dto, user))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateMagicNumber(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());
        // Decode check fails.
        var decodeFailure = ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFileType);
        _audioFileValidatorMock
            .Setup(x => x.ValidateByDecodingWithFfmpeg(dto.AudioFile))
            .Returns(decodeFailure);

        // Act
        var result = _helper.ValidateAudioFileCreateRequest(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidAudioFileType);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void ValidateAudioFileCreateRequest_DevelopmentMode_ReturnsSuccessWithoutVirusScan()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", new byte[1024]);
        var user = CreateDummyUser();

        // Set environment variable so DEVELOPMENT is true.
        Environment.SetEnvironmentVariable("DEVELOPMENT", "true");

        _audioFileValidatorMock
            .Setup(x => x.ValidateFileBasics(dto, user))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateMagicNumber(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateByDecodingWithFfmpeg(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());

        // Act
        var result = _helper.ValidateAudioFileCreateRequest(dto, user);

        // Clean up environment variable.
        Environment.SetEnvironmentVariable("DEVELOPMENT", null);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateAudioFileCreateRequest_VirusScanFails_ReturnsFailure()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", new byte[1024]);
        var user = CreateDummyUser();

        // Set DEVELOPMENT to false.
        Environment.SetEnvironmentVariable("DEVELOPMENT", "false");

        _audioFileValidatorMock
            .Setup(x => x.ValidateFileBasics(dto, user))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateMagicNumber(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateByDecodingWithFfmpeg(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());

        var virusFailure = ServiceResult<object>.Failure(MessageKey.Error_MalwareOrVirusDetected);
        _fileSafetyValidatorMock
            .Setup(x => x.ScanWithClamAV(dto.AudioFile))
            .ReturnsAsync(virusFailure);

        // Act
        var result = _helper.ValidateAudioFileCreateRequest(dto, user);

        Environment.SetEnvironmentVariable("DEVELOPMENT", null);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_MalwareOrVirusDetected);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public void ValidateAudioFileCreateRequest_AllChecksPass_ReturnsSuccess()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", new byte[1024]);
        var user = CreateDummyUser();

        Environment.SetEnvironmentVariable("DEVELOPMENT", "false");

        _audioFileValidatorMock
            .Setup(x => x.ValidateFileBasics(dto, user))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateMagicNumber(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());
        _audioFileValidatorMock
            .Setup(x => x.ValidateByDecodingWithFfmpeg(dto.AudioFile))
            .Returns(ServiceResult<object>.SuccessResult());
        _fileSafetyValidatorMock
            .Setup(x => x.ScanWithClamAV(dto.AudioFile))
            .ReturnsAsync(ServiceResult<object>.SuccessResult());

        // Act
        var result = _helper.ValidateAudioFileCreateRequest(dto, user);

        Environment.SetEnvironmentVariable("DEVELOPMENT", null);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region ValidateAudioFileWithUserAsync Tests

    [Fact]
    public async Task ValidateAudioFileWithUserAsync_Success_ReturnsSuccess()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();

        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<bool>.Success(true));

        // Act
        var result = await _helper.ValidateAudioFileWithUserAsync(audioId, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAudioFileWithUserAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();

        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<bool>.NotFound());

        // Act
        var result = await _helper.ValidateAudioFileWithUserAsync(audioId, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_Unauthorized);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateAudioFileWithUserAsync_Error_ReturnsFailure()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();

        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<bool>.Error());

        // Act
        var result = await _helper.ValidateAudioFileWithUserAsync(audioId, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task ValidateAudioFileWithUserAsync_Exception_ReturnsFailure()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();

        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _helper.ValidateAudioFileWithUserAsync(audioId, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorService);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region CreateAudioFileAsync Tests

    [Fact]
    public async Task CreateAudioFileAsync_SuccessfulCreation_ReturnsSuccessResponse()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", Encoding.UTF8.GetBytes("dummy content"));
        var user = CreateDummyUser();

        // Simulate conversion from DTO to AudioFile.
        // Assuming ToAudioFromCreateDTO creates an AudioFile with properties set from the DTO.
        var audio = new AudioFile 
        { 
            Id = Guid.NewGuid(), 
            Name = dto.Name, 
            FileSize = dto.AudioFile.Length, 
            UserId = user.Id, 
            CreatedAt = DateTime.UtcNow 
        };

        _audioDataMock.Setup(x => x.SaveAudioFileAsync(It.IsAny<AudioFile>()))
            .ReturnsAsync(DataResult<AudioFile>.Success(audio));

        // Act
        var result = await _helper.CreateAudioFileAsync(dto, user);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // Cleanup: delete the created file if it exists.
        var userFolderPath = Path.Combine(_uploadsFolderPath, user.Id.ToString());
        var filePath = Path.Combine(userFolderPath, $"{audio.Id}.mp3");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task CreateAudioFileAsync_SaveAudioFileMetadataFails_ReturnsFailure()
    {
        // Arrange
        var dto = CreateDummyAudioFileCreateDTO("test.mp3", "Test Audio", Encoding.UTF8.GetBytes("dummy content"));
        var user = CreateDummyUser();

        var audio = new AudioFile 
        { 
            Id = Guid.NewGuid(), 
            Name = dto.Name, 
            FileSize = dto.AudioFile.Length, 
            UserId = user.Id, 
            CreatedAt = DateTime.UtcNow 
        };

        _audioDataMock.Setup(x => x.SaveAudioFileAsync(It.IsAny<AudioFile>()))
            .ReturnsAsync(DataResult<AudioFile>.Error());

        // Act
        var result = await _helper.CreateAudioFileAsync(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_UnableToSaveAudioFileMetaData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion

    #region DeleteAudioFileAsync Tests

    [Fact]
    public async Task DeleteAudioFileAsync_SuccessfulDeletion_ReturnsSuccess()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();
        var removeValue = 1L;
        var removeResult = DataResult<long>.Success(removeValue);

        // Setup ValidateAudioFileWithUserAsync via AudioFileBelongsToUserAsync.
        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<bool>.Success(true));

        _audioDataMock.Setup(x => x.RemoveAudioFileAsync(audioId, user.Id))
            .ReturnsAsync(removeResult);

        // Create a dummy file to simulate an existing file.
        var userFolderPath = Path.Combine(_uploadsFolderPath, user.Id.ToString());
        Directory.CreateDirectory(userFolderPath);
        var filePath = Path.Combine(userFolderPath, $"{audioId}.mp3");
        File.WriteAllText(filePath, "dummy content");

        // Act
        var result = await _helper.DeleteAudioFileAsync(new AudioFileRemoveDTO { AudioId = audioId }, user);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(removeValue, result.Data);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteAudioFileAsync_ValidationFails_ReturnsFailure()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();

        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<bool>.NotFound());

        // Act
        var result = await _helper.DeleteAudioFileAsync(new AudioFileRemoveDTO { AudioId = audioId }, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_Unauthorized);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task DeleteAudioFileAsync_RemoveAudioFails_ReturnsFailure()
    {
        // Arrange
        var audioId = Guid.NewGuid();
        var user = CreateDummyUser();

        _audioDataMock.Setup(x => x.AudioFileBelongsToUserAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<bool>.Success(true));

        _audioDataMock.Setup(x => x.RemoveAudioFileAsync(audioId, user.Id))
            .ReturnsAsync(DataResult<long>.Error());

        // Act
        var result = await _helper.DeleteAudioFileAsync(new AudioFileRemoveDTO { AudioId = audioId }, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    #endregion
}