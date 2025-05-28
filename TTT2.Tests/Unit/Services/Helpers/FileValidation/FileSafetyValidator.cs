using Microsoft.AspNetCore.Http;
using TTT2.Services.Helpers.FileValidation;

namespace TTT2.Tests.Unit.Services.Helpers.FileValidation;

[Trait("Category", "Unit")]
public class FileSafetyValidatorTests
{
    private readonly FileSafetyValidator _validator = new();

    [Fact]
    public async Task ScanWithClamAV_WithCleanFile_ReturnsSuccess()
    {
        // Arrange: Set up the path to the test file.
        var currentDirectory = Directory.GetCurrentDirectory();
        var assetsFolder = Path.Combine(currentDirectory, "Assets");
        var filePath = Path.Combine(assetsFolder, "TestAudioFile.mp3");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Test audio file not found.", filePath);
        }

        // Read file into a byte array.
        var fileBytes = await File.ReadAllBytesAsync(filePath);

        // Prepare a stream from the file bytes.
        var fileStream = new MemoryStream(fileBytes);
        // Create an IFormFile instance.
        IFormFile formFile = new FormFile(fileStream, 0, fileBytes.Length, "AudioFile", Path.GetFileName(filePath))
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/mpeg"
        };

        // Act: Perform the scan using your ClamAV validator.
        // This will call your method that writes the file to a temporary file, then scans it via ClamAV.
        var result = await _validator.ScanWithClamAV(formFile);

        // Assert: Expect a success result from a clean file.
        Assert.True(result.IsSuccess, "Expected file to be scanned as clean, but a malware or virus was detected. Ensure ClamAV is running on localhost:3310.");
    }
}