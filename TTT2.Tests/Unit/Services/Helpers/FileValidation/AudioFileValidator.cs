using System.Text;
using Microsoft.AspNetCore.Http;
using Shared.Enums;
using Shared.Models;
using Shared.Models.DTOs.AudioFiles;
using Shared.Statics;
using TTT2.Services.Helpers.FileValidation;

namespace TTT2.Tests.Unit.Services.Helpers.FileValidation;

[Trait("Category", "Unit")]
public class AudioFileValidatorTests
{
    private readonly AudioFileValidator _validator = new AudioFileValidator();

    // Helper method to create a dummy IFormFile from a byte array.
    private IFormFile CreateTestFormFile(byte[] content, string fileName)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName);
    }

    // Helper method to create a dummy User with all required properties.
    private User CreateDummyUser(long usedStorageBytes = 0, long maxStorageBytes = 10 * 1024 * 1024)
    {
        return new User
        {
            Username = "dummyUser",
            Email = "dummy@example.com",
            PasswordHash = "dummyHash",
            UsedStorageBytes = usedStorageBytes,
            MaxStorageBytes = maxStorageBytes,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void ValidateFileBasics_ValidInput_ReturnsSuccess()
    {
        // Arrange: Create a 1KB dummy file with valid properties.
        byte[] dummyContent = new byte[1024]; // 1KB file
        var formFile = CreateTestFormFile(dummyContent, "test.mp3");
        var dto = new AudioFileCreateDTO { AudioFile = formFile, Name = "Test Audio" };
        var user = CreateDummyUser();

        // Act
        var result = _validator.ValidateFileBasics(dto, user);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateFileBasics_EmptyName_ReturnsFailureInvalidInput()
    {
        // Arrange: Name is empty.
        byte[] dummyContent = new byte[1024];
        var formFile = CreateTestFormFile(dummyContent, "test.mp3");
        var dto = new AudioFileCreateDTO { AudioFile = formFile, Name = "" };
        var user = CreateDummyUser();

        // Act
        var result = _validator.ValidateFileBasics(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void ValidateFileBasics_FileLengthZero_ReturnsFailureInvalidAudioFile()
    {
        // Arrange: File length is zero.
        byte[] dummyContent = new byte[0];
        var formFile = CreateTestFormFile(dummyContent, "test.mp3");
        var dto = new AudioFileCreateDTO { AudioFile = formFile, Name = "Test Audio" };
        var user = CreateDummyUser();

        // Act
        var result = _validator.ValidateFileBasics(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_InvalidAudioFile);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void ValidateFileBasics_FileExceedsMaxSize_ReturnsFailureFileTooLarge()
    {
        // Arrange: Create a file larger than allowed.
        byte[] dummyContent = new byte[6 * 1024 * 1024]; // 6MB file
        var formFile = CreateTestFormFile(dummyContent, "test.mp3");
        var dto = new AudioFileCreateDTO { AudioFile = formFile, Name = "Test Audio" };
        var user = CreateDummyUser();

        // Act
        var result = _validator.ValidateFileBasics(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_FileTooLarge);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void ValidateFileBasics_ExceedsUserStorageQuota_ReturnsFailureExceedsStorageQuota()
    {
        // Arrange: Create a file that makes user's storage quota exceed.
        byte[] dummyContent = new byte[1024]; // 1KB file
        var formFile = CreateTestFormFile(dummyContent, "test.mp3");
        var dto = new AudioFileCreateDTO { AudioFile = formFile, Name = "Test Audio" };

        // User already used almost all allowed storage.
        var user = CreateDummyUser(usedStorageBytes: 10 * 1024 * 1024 - 512, maxStorageBytes: 10 * 1024 * 1024);

        // Act
        var result = _validator.ValidateFileBasics(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_ExceedsStorageQuota);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void ValidateFileBasics_InvalidFileExtension_ReturnsFailureInvalidAudioFileType()
    {
        // Arrange: Create file with an invalid extension.
        byte[] dummyContent = new byte[1024];
        var formFile = CreateTestFormFile(dummyContent, "test.wav"); // Not .mp3
        var dto = new AudioFileCreateDTO { AudioFile = formFile, Name = "Test Audio" };
        var user = CreateDummyUser();

        // Act
        var result = _validator.ValidateFileBasics(dto, user);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_InvalidAudioFileType);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }

    [Fact]
    public void ValidateMagicNumber_ValidMagicNumber_ReturnsSuccess()
    {
        // Arrange: Create file content that starts with the magic number "ID3".
        byte[] fileBytes = Encoding.ASCII.GetBytes("ID3ExtraData");
        var formFile = CreateTestFormFile(fileBytes, "test.mp3");

        // Act
        var result = _validator.ValidateMagicNumber(formFile);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateMagicNumber_InvalidMagicNumber_ReturnsFailure()
    {
        // Arrange: Create file content with an incorrect magic number.
        byte[] fileBytes = Encoding.ASCII.GetBytes("XYZExtraData");
        var formFile = CreateTestFormFile(fileBytes, "test.mp3");

        // Act
        var result = _validator.ValidateMagicNumber(formFile);

        // Assert
        Assert.False(result.IsSuccess);
        var expectedMessage = MessageRepository.GetMessage(MessageKey.Error_InvalidAudioFileType);
        Assert.Equal(expectedMessage.InternalMessage, result.MessageInfo.InternalMessage);
        Assert.Equal(expectedMessage.Type, result.MessageInfo.Type);
        Assert.Equal(expectedMessage.HttpStatusCode, result.MessageInfo.HttpStatusCode);
    }
}