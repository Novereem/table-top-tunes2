using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Enums;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Statics;
using TTT2.Services.Helpers;

namespace TTT2.Tests.Unit.Services.Helpers
{
    public class AudioStreamingServiceHelperTests : IDisposable
    {
        private readonly Mock<IAudioServiceHelper> _audioServiceHelperMock;
        private readonly AudioStreamingServiceHelper _helper;
        private readonly string _uploadsRoot;

        public AudioStreamingServiceHelperTests()
        {
            _audioServiceHelperMock = new Mock<IAudioServiceHelper>();
            _helper = new AudioStreamingServiceHelper(_audioServiceHelperMock.Object);

            // Ensure a clean Uploads folder in the test working directory
            _uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (Directory.Exists(_uploadsRoot))
                Directory.Delete(_uploadsRoot, true);
            Directory.CreateDirectory(_uploadsRoot);
        }

        public void Dispose()
        {
            if (Directory.Exists(_uploadsRoot))
                Directory.Delete(_uploadsRoot, true);
        }

        [Fact]
        public void ValidateAndBuildPath_OwnershipFails_ReturnsFailure()
        {
            var audioId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _audioServiceHelperMock
                .Setup(s => s.ValidateAudioFileWithUserAsync(audioId, userId))
                .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized));

            var result = _helper.ValidateAndBuildPath(audioId, userId);

            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_Unauthorized);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }

        [Fact]
        public void ValidateAndBuildPath_FileMissing_ReturnsNotFound()
        {
            var audioId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _audioServiceHelperMock
                .Setup(s => s.ValidateAudioFileWithUserAsync(audioId, userId))
                .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

            // No file on disk

            var result = _helper.ValidateAndBuildPath(audioId, userId);

            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }

        [Fact]
        public void ValidateAndBuildPath_FileExists_ReturnsPathAndSize()
        {
            var audioId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _audioServiceHelperMock
                .Setup(s => s.ValidateAudioFileWithUserAsync(audioId, userId))
                .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

            // Create the file
            var userFolder = Path.Combine(_uploadsRoot, userId.ToString());
            Directory.CreateDirectory(userFolder);
            var filePath = Path.Combine(userFolder, $"{audioId}.mp3");
            File.WriteAllText(filePath, "hello world");

            var result = _helper.ValidateAndBuildPath(audioId, userId);

            Assert.True(result.IsSuccess);
            Assert.Equal(filePath, result.Data.physicalPath);
            Assert.Equal(new FileInfo(filePath).Length, result.Data.fileSize);
        }

        [Fact]
        public void BuildFullStream_CreatesCorrectDto()
        {
            // Create a temp file
            var filePath = Path.Combine(_uploadsRoot, "f1.mp3");
            var content = Encoding.ASCII.GetBytes("abcdefgh");
            File.WriteAllBytes(filePath, content);

            var fileSize = content.Length;
            var res = _helper.BuildFullStream(filePath, fileSize);

            Assert.True(res.IsSuccess);
            var dto = res.Data!;
            Assert.Equal("audio/mpeg", dto.ContentType);
            Assert.Equal(StatusCodes.Status200OK, dto.StatusCode);
            Assert.Equal(fileSize, dto.ContentLength);
            Assert.Equal("bytes", dto.Headers.AcceptRanges);
            Assert.Null(dto.Headers.ContentRange);

            // Read back stream
            using var stream = dto.FileStream;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var read = ms.ToArray();
            Assert.Equal(content, read);
        }

        [Fact]
        public void BuildPartialStream_InvalidHeader_FallsBackToFull()
        {
            // Create a temp file
            var filePath = Path.Combine(_uploadsRoot, "f2.mp3");
            var content = Encoding.ASCII.GetBytes("0123456789");
            File.WriteAllBytes(filePath, content);

            var res = _helper.BuildPartialStream(Guid.NewGuid(), filePath, content.Length, "not-a-range");

            Assert.True(res.IsSuccess);
            var dto = res.Data!;
            Assert.Equal(StatusCodes.Status200OK, dto.StatusCode);
            Assert.Equal(content.Length, dto.ContentLength);
            
            using (dto.FileStream) { }
        }

        [Fact]
        public void BuildPartialStream_ValidRange_ReturnsSlice()
        {
            // Create a temp file
            var filePath = Path.Combine(_uploadsRoot, "f3.mp3");
            var content = Encoding.ASCII.GetBytes("ABCDEFGHIJ"); // 10 bytes
            File.WriteAllBytes(filePath, content);

            // Request bytes 2-5 ("CDEF")
            var res = _helper.BuildPartialStream(Guid.NewGuid(), filePath, content.Length, "bytes=2-5");

            Assert.True(res.IsSuccess);
            var dto = res.Data!;
            Assert.Equal(StatusCodes.Status206PartialContent, dto.StatusCode);
            Assert.Equal(4, dto.ContentLength);  // 5-2+1
            Assert.Equal("bytes", dto.Headers.AcceptRanges);
            Assert.Equal($"bytes 2-5/{content.Length}", dto.Headers.ContentRange);

            // Read slice
            using var stream = dto.FileStream;
            using var ms     = new MemoryStream();
            stream.CopyTo(ms);
            var slice = ms.ToArray();
            Assert.Equal("CDEF"u8.ToArray(), slice);
        }

        [Fact]
        public void BuildPartialStream_RangeStartBeyondSize_ReturnsFailure()
        {
            // Create a temp file
            var filePath = Path.Combine(_uploadsRoot, "f4.mp3");
            File.WriteAllBytes(filePath, new byte[5]); // length=5

            var res = _helper.BuildPartialStream(Guid.NewGuid(), filePath, 5, "bytes=10-15");

            Assert.False(res.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_StreamRangeNotSatisfiable);
            Assert.Equal(expected.InternalMessage, res.MessageInfo.InternalMessage);
        }
    }
}