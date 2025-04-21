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

namespace TTT2.Tests.Unit.Services
{
    public class AudioStreamingServiceTests
    {
        private readonly Mock<IUserClaimsService> _userClaimsServiceMock;
        private readonly Mock<IAudioStreamingServiceHelper> _helperMock;
        private readonly AudioStreamingService _service;

        public AudioStreamingServiceTests()
        {
            _userClaimsServiceMock = new Mock<IUserClaimsService>();
            _helperMock           = new Mock<IAudioStreamingServiceHelper>();
            _service              = new AudioStreamingService(
                _userClaimsServiceMock.Object,
                _helperMock.Object
            );
        }

        // Helper to create a valid DTO and principal
        private static AudioStreamDTO CreateDto() => new() { AudioId = Guid.NewGuid() };
        private static ClaimsPrincipal CreatePrincipalWithId(Guid userId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async void StreamAudioAsync_UserValidationFails_ReturnsFailure()
        {
            var dto    = CreateDto();
            var user   = new ClaimsPrincipal();
            var failure = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);

            _userClaimsServiceMock
                .Setup(m => m.GetUserIdFromClaims(user))
                .Returns(failure);

            var result = await _service.StreamAudioAsync(dto, user, null);

            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }

        [Fact]
        public async void StreamAudioAsync_PathValidationFails_ReturnsFailure()
        {
            var dto    = CreateDto();
            var userId = Guid.NewGuid();
            var user   = CreatePrincipalWithId(userId);

            _userClaimsServiceMock
                .Setup(m => m.GetUserIdFromClaims(user))
                .Returns(ServiceResult<Guid>.SuccessResult(userId));

            _helperMock
                .Setup(h => h.ValidateAndBuildPath(dto.AudioId, userId))
                .Returns(ServiceResult<(string, long)>.Failure(MessageKey.Error_NotFound));

            var result = await _service.StreamAudioAsync(dto, user, null);

            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }

        [Fact]
        public async void StreamAudioAsync_NoRange_UsesBuildFullStream()
        {
            var dto      = CreateDto();
            var userId   = Guid.NewGuid();
            var user     = CreatePrincipalWithId(userId);
            const string path = "/tmp/fake.mp3";
            const long size = 12345L;
            var expectedDto = new AudioStreamResponseDTO
            {
                FileStream    = Stream.Null,
                ContentType   = "audio/mpeg",
                ContentLength = size,
                StatusCode    = StatusCodes.Status200OK,
                Headers       = new AudioStreamHeadersDTO()
            };

            _userClaimsServiceMock
                .Setup(m => m.GetUserIdFromClaims(user))
                .Returns(ServiceResult<Guid>.SuccessResult(userId));

            _helperMock
                .Setup(h => h.ValidateAndBuildPath(dto.AudioId, userId))
                .Returns(ServiceResult<(string, long)>.SuccessResult((path, size)));

            _helperMock
                .Setup(h => h.BuildFullStream(path, size))
                .Returns(ServiceResult<AudioStreamResponseDTO>.SuccessResult(expectedDto));

            var result = await _service.StreamAudioAsync(dto, user, rangeHeader: null);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDto.StatusCode, result.Data.StatusCode);
            Assert.Equal(expectedDto.ContentLength, result.Data.ContentLength);
        }

        [Fact]
        public async void StreamAudioAsync_WithRange_UsesBuildPartialStream()
        {
            var dto      = CreateDto();
            var userId   = Guid.NewGuid();
            var user     = CreatePrincipalWithId(userId);
            const string path = "/tmp/fake.mp3";
            const long size = 5000L;
            const string range = "bytes=0-100";
            var expectedDto = new AudioStreamResponseDTO
            {
                FileStream    = Stream.Null,
                ContentType   = "audio/mpeg",
                ContentLength = 101,
                StatusCode    = StatusCodes.Status206PartialContent,
                Headers       = new AudioStreamHeadersDTO { AcceptRanges = "bytes", ContentRange = $"bytes 0-100/{size}" }
            };

            _userClaimsServiceMock
                .Setup(m => m.GetUserIdFromClaims(user))
                .Returns(ServiceResult<Guid>.SuccessResult(userId));

            _helperMock
                .Setup(h => h.ValidateAndBuildPath(dto.AudioId, userId))
                .Returns(ServiceResult<(string, long)>.SuccessResult((path, size)));

            _helperMock
                .Setup(h => h.BuildPartialStream(dto.AudioId, path, size, range))
                .Returns(ServiceResult<AudioStreamResponseDTO>.SuccessResult(expectedDto));

            var result = await _service.StreamAudioAsync(dto, user, rangeHeader: range);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDto.StatusCode, result.Data.StatusCode);
            Assert.Equal(expectedDto.Headers.ContentRange, result.Data.Headers.ContentRange);
        }

        [Fact]
        public async void StreamAudioAsync_StreamHelperFailure_ReturnsFailure()
        {
            var dto      = CreateDto();
            var userId   = Guid.NewGuid();
            var user     = CreatePrincipalWithId(userId);
            const string path = "/tmp/doesntmatter";
            const long size = 1L;
            const MessageKey errKey = MessageKey.Error_StreamRangeNotSatisfiable;

            _userClaimsServiceMock
                .Setup(m => m.GetUserIdFromClaims(user))
                .Returns(ServiceResult<Guid>.SuccessResult(userId));

            _helperMock
                .Setup(h => h.ValidateAndBuildPath(dto.AudioId, userId))
                .Returns(ServiceResult<(string, long)>.SuccessResult((path, size)));

            // Simulate partial‐stream failure
            _helperMock
                .Setup(h => h.BuildPartialStream(dto.AudioId, path, size, It.IsAny<string>()))
                .Returns(ServiceResult<AudioStreamResponseDTO>.Failure(errKey));

            var result = await _service.StreamAudioAsync(dto, user, rangeHeader: "bytes=0-0");

            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(errKey);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }

        [Fact]
        public async void StreamAudioAsync_HelperThrows_ExceptionHandledAsInternalError()
        {
            var dto    = CreateDto();
            var userId = Guid.NewGuid();
            var user   = CreatePrincipalWithId(userId);

            _userClaimsServiceMock
                .Setup(m => m.GetUserIdFromClaims(user))
                .Returns(ServiceResult<Guid>.SuccessResult(userId));

            _helperMock
                .Setup(h => h.ValidateAndBuildPath(dto.AudioId, userId))
                .Throws(new InvalidOperationException("boom"));

            var result = await _service.StreamAudioAsync(dto, user, rangeHeader: null);

            Assert.False(result.IsSuccess);
            var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerError);
            Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
        }
    }
}