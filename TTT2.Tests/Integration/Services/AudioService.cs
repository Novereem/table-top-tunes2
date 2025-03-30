using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.Authentication;
using TTT2.Services;

public class FakeUserClaimsServiceAudioService : IUserClaimsService
{
    public ServiceResult<Guid> GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return ServiceResult<Guid>.Failure(MessageKey.Error_InvalidInput);
        return ServiceResult<Guid>.SuccessResult(userId);
    }
}

public class FakeAuthenticationService : IAuthenticationService
{
    public Task<HttpServiceResult<RegisterResponseDTO>> RegisterUserAsync(RegisterDTO registerDTO)
    {
        throw new NotImplementedException();
    }

    public Task<HttpServiceResult<LoginResponseDTO>> LoginUserAsync(LoginDTO loginDTO)
    {
        throw new NotImplementedException();
    }

    public Task<HttpServiceResult<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserDTO updateUserDTO,
        ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
    {
        // Simulate finding a user in the in-memory data store.
        var user = new User
        {
            Id = userId,
            Username = "TestUser",
            Email = "test@example.com",
            PasswordHash = "fakehash",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024, // e.g., 10 MB
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<User>.SuccessResult(user));
    }
}

public class FakeUserStorageService : IUserStorageService
{
    // Using a concurrent dictionary to track storage usage per user.
    private readonly ConcurrentDictionary<Guid, long> _storage = new();

    public Task<ServiceResult<bool>> IncreaseUserStorageAsync(Guid userId, long additionalBytes)
    {
        _storage.AddOrUpdate(userId, additionalBytes, (id, current) => current + additionalBytes);
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
    }

    public Task<ServiceResult<bool>> DecreaseUserStorageAsync(Guid userId, long bytes)
    {
        _storage.AddOrUpdate(userId, 0, (id, current) => current - bytes);
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
    }
}

public class FakeAudioServiceHelper : IAudioServiceHelper
{
    // In-memory storage for one audio file (for simplicity)
    private AudioFile _storedAudio;

    public ServiceResult<object> ValidateAudioFileCreateRequest(AudioFileCreateDTO dto, User user)
    {
        if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult<object>.Failure(MessageKey.Error_InvalidAudioFile);

        return ServiceResult<object>.SuccessResult(null, MessageKey.Success_AudioCreation);
    }

    public async Task<ServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(AudioFileCreateDTO dto, User user)
    {
        // Simulate the creation of an audio file.
        _storedAudio = new AudioFile
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            UserId = user.Id,
            FileSize = dto.AudioFile.Length,
            CreatedAt = DateTime.UtcNow
        };

        var response = new AudioFileCreateResponseDTO
        {
            Id = _storedAudio.Id,
            Name = _storedAudio.Name,
            UserId = _storedAudio.UserId,
            CreatedAt = _storedAudio.CreatedAt
        };

        return await Task.FromResult(
            ServiceResult<AudioFileCreateResponseDTO>.SuccessResult(response, MessageKey.Success_AudioCreation));
    }

    public async Task<ServiceResult<long>> DeleteAudioFileAsync(AudioFileRemoveDTO dto, User user)
    {
        if (_storedAudio != null && _storedAudio.Id == dto.AudioId && _storedAudio.UserId == user.Id)
        {
            var fileSize = (int)_storedAudio.FileSize;
            _storedAudio = null;
            return await Task.FromResult(ServiceResult<long>.SuccessResult(fileSize, MessageKey.Success_AudioRemoval));
        }

        return await Task.FromResult(ServiceResult<long>.Failure(MessageKey.Error_NotFound));
    }

    public async Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId)
    {
        if (_storedAudio != null && _storedAudio.Id == audioId && _storedAudio.UserId == userId)
            return await Task.FromResult(ServiceResult<bool>.SuccessResult(true));
        return await Task.FromResult(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));
    }
}

// Integration tests for AudioService

namespace TTT2.IntegrationTests
{
    public class AudioServiceIntegrationTests
    {
        private readonly IAudioServiceHelper _audioHelper = new FakeAudioServiceHelper();
        private readonly AudioService _audioService;
        private readonly IAuthenticationService _authenticationService = new FakeAuthenticationService();
        private readonly ClaimsPrincipal _testUser;
        private readonly Guid _testUserId = Guid.NewGuid();

        // Instantiate fake implementations without mocks.
        private readonly IUserClaimsService _userClaimsService = new FakeUserClaimsServiceAudioService();
        private readonly IUserStorageService _userStorageService = new FakeUserStorageService();

        public AudioServiceIntegrationTests()
        {
            // Create a test user with the NameIdentifier claim.
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()));
            _testUser = new ClaimsPrincipal(identity);

            // Instantiate AudioService with the fake (in-memory) implementations.
            _audioService = new AudioService(_userClaimsService, _audioHelper, _authenticationService,
                _userStorageService);
        }

        [Fact]
        public async Task CreateAudioFileAsync_Successful_Test()
        {
            // Arrange: Create an AudioFileCreateDTO with a fake IFormFile.
            var fileContent = "Fake audio content";
            var fileName = "test_audio.mp3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            IFormFile formFile = new FormFile(stream, 0, stream.Length, "audio", fileName);

            var audioCreateDTO = new AudioFileCreateDTO
            {
                AudioFile = formFile,
                Name = "Test Audio"
            };

            // Act: Call CreateAudioFileAsync.
            var result = await _audioService.CreateAudioFileAsync(audioCreateDTO, _testUser);

            // Assert: Verify the operation succeeded and the returned data is valid.
            Assert.True(result.IsSuccess, "Audio creation should succeed.");
            Assert.NotNull(result.Data);
            Assert.Equal("Test Audio", result.Data.Name);
            Assert.Equal(_testUserId, result.Data.UserId);
            Assert.NotEqual(Guid.Empty, result.Data.Id);
        }

        [Fact]
        public async Task DeleteAudioFileAsync_Successful_Test()
        {
            // Arrange: Create an audio file first.
            var fileContent = "Fake audio content for deletion";
            var fileName = "delete_audio.mp3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            IFormFile formFile = new FormFile(stream, 0, stream.Length, "audio", fileName);

            var audioCreateDTO = new AudioFileCreateDTO
            {
                AudioFile = formFile,
                Name = "Audio to Delete"
            };

            var createResult = await _audioService.CreateAudioFileAsync(audioCreateDTO, _testUser);
            Assert.True(createResult.IsSuccess, "Setup audio creation failed.");

            // Act: Delete the audio file using its Id.
            var audioRemoveDTO = new AudioFileRemoveDTO { AudioId = createResult.Data.Id };
            var deleteResult = await _audioService.DeleteAudioFileAsync(audioRemoveDTO, _testUser);

            // Assert: Verify that deletion succeeded.
            Assert.True(deleteResult.IsSuccess, "Audio deletion should succeed.");
            Assert.True(deleteResult.Data);
        }

        [Fact]
        public async Task ValidateAudioFileWithUserAsync_Successful_Test()
        {
            // Arrange: Create an audio file.
            var fileContent = "Fake audio content for validation";
            var fileName = "validate_audio.mp3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            IFormFile formFile = new FormFile(stream, 0, stream.Length, "audio", fileName);

            var audioCreateDTO = new AudioFileCreateDTO
            {
                AudioFile = formFile,
                Name = "Audio to Validate"
            };

            var createResult = await _audioService.CreateAudioFileAsync(audioCreateDTO, _testUser);
            Assert.True(createResult.IsSuccess, "Setup audio creation failed.");

            // Act: Validate that the created audio belongs to the test user.
            var validationResult =
                await _audioService.ValidateAudioFileWithUserAsync(createResult.Data.Id, _testUserId);

            // Assert: Check that validation returns a successful result.
            Assert.True(validationResult.IsSuccess, "Validation should succeed.");
            Assert.True(validationResult.Data);
        }
    }
}