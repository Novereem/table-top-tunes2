using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.Enums;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioStreaming;
using Shared.Statics;
using TTT2.Services;
using TTT2.Services.Helpers;

namespace TTT2.Tests.Integration.Services
{
    public class AudioStreamingServiceIntegrationTests : IDisposable
    {
        private readonly string _uploadsRoot;
        private readonly Guid   _userId;
        private readonly ClaimsPrincipal _user;
        private readonly AudioStreamingService _streamService;

        public AudioStreamingServiceIntegrationTests()
        {
            // Set up temp Uploads folder in test directory
            _uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (Directory.Exists(_uploadsRoot))
                Directory.Delete(_uploadsRoot, true);
            Directory.CreateDirectory(_uploadsRoot);

            // Test user
            _userId = Guid.NewGuid();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _userId.ToString()));
            _user = new ClaimsPrincipal(identity);

            // Fake out only the ownership check
            var ownershipHelper = new Mock<IAudioServiceHelper>();
            ownershipHelper
                .Setup(h => h.ValidateAudioFileWithUserAsync(It.IsAny<Guid>(), _userId))
                .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

            // Real streaming helper and service
            var streamHelper = new AudioStreamingServiceHelper(ownershipHelper.Object);
            _streamService = new AudioStreamingService(
                new FakeUserClaimsService(), 
                streamHelper
            );
        }

        public void Dispose()
        {
            if (Directory.Exists(_uploadsRoot))
                Directory.Delete(_uploadsRoot, true);
        }

        private void WriteTestAudioFile(Guid audioId, byte[] content)
        {
            var userFolder = Path.Combine(_uploadsRoot, _userId.ToString());
            Directory.CreateDirectory(userFolder);
            var path = Path.Combine(userFolder, $"{audioId}.mp3");
            File.WriteAllBytes(path, content);
        }

        [Fact]
        public async Task StreamAudioAsync_FullFile_Returns200AndCorrectBytes()
        {
            // Arrange
            var audioId = Guid.NewGuid();
            var data    = "HELLOWORLD"u8.ToArray();
            WriteTestAudioFile(audioId, data);

            var dto = new AudioStreamDTO { AudioId = audioId };

            // Act
            var result = await _streamService.StreamAudioAsync(dto, _user, rangeHeader: null);

            // Assert
            Assert.True(result.IsSuccess);
            var resDto = result.Data!;
            Assert.Equal(StatusCodes.Status200OK,      resDto.StatusCode);
            Assert.Equal("audio/mpeg",                 resDto.ContentType);
            Assert.Equal(data.Length,                  resDto.ContentLength);
            Assert.Equal("bytes",                      resDto.Headers.AcceptRanges);
            Assert.Null(resDto.Headers.ContentRange);

            // Read and verify bytes
            using var ms = new MemoryStream();
            await resDto.FileStream.CopyToAsync(ms);
            Assert.Equal(data, ms.ToArray());
            resDto.FileStream.Dispose();
        }

        [Fact]
        public async Task StreamAudioAsync_PartialFile_Returns206AndSlice()
        {
            // Arrange
            var audioId = Guid.NewGuid();
            var data    = "ABCDEFGHIJ"u8.ToArray(); // 10 bytes
            WriteTestAudioFile(audioId, data);

            var dto = new AudioStreamDTO { AudioId = audioId };
            var range = "bytes=2-5"; // expects "CDEF"

            // Act
            var result = await _streamService.StreamAudioAsync(dto, _user, rangeHeader: range);

            // Assert
            Assert.True(result.IsSuccess);
            var resDto = result.Data!;
            Assert.Equal(StatusCodes.Status206PartialContent, resDto.StatusCode);
            Assert.Equal(4, resDto.ContentLength);
            Assert.Equal("bytes",                     resDto.Headers.AcceptRanges);
            Assert.Equal($"bytes 2-5/{data.Length}", resDto.Headers.ContentRange);

            using var ms = new MemoryStream();
            await resDto.FileStream.CopyToAsync(ms);
            Assert.Equal("CDEF"u8.ToArray(), ms.ToArray());
            resDto.FileStream.Dispose();
        }

        [Fact]
        public async Task StreamAudioAsync_FileNotFound_ReturnsNotFoundError()
        {
            // Arrange
            var audioId = Guid.NewGuid();
            // no file written
            var dto = new AudioStreamDTO { AudioId = audioId };

            // Act
            var result = await _streamService.StreamAudioAsync(dto, _user, rangeHeader: null);

            // Assert
            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }

        // Minimal fake for user‐claims
        private class FakeUserClaimsService : IUserClaimsService
        {
            public ServiceResult<Guid> GetUserIdFromClaims(ClaimsPrincipal user)
            {
                var c = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (c != null && Guid.TryParse(c, out var id))
                    return ServiceResult<Guid>.SuccessResult(id);
                return ServiceResult<Guid>.Failure(MessageKey.Error_Unauthorized);
            }
        }
    }
}