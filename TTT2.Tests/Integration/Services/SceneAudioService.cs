using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.SceneAudios;
using TTT2.Services;

public class FakeUserClaimsService : IUserClaimsService
{
    public ServiceResult<Guid> GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            return ServiceResult<Guid>.Failure(MessageKey.Error_InvalidInput);
        return ServiceResult<Guid>.SuccessResult(userId);
    }
}

public class FakeAudioService : IAudioService
{
    public Task<HttpServiceResult<AudioFileCreateResponseDTO>> CreateAudioFileAsync(
        AudioFileCreateDTO audioFileCreateDTO, ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }

    public Task<HttpServiceResult<bool>> DeleteAudioFileAsync(AudioFileRemoveDTO audioFileRemoveDTO,
        ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<bool>> ValidateAudioFileWithUserAsync(Guid audioId, Guid userId)
    {
        if (audioId == Guid.Empty)
            return Task.FromResult(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
    }
}

public class FakeSceneAudioServiceHelper : ISceneAudioServiceHelper
{
    public Task<ServiceResult<SceneAudioAssignResponseDTO>> AddSceneAudioFileAsync(SceneAudioAssignDTO dto)
    {
        var response = new SceneAudioAssignResponseDTO
        {
            SceneId = dto.SceneId,
            AudioFileId = dto.AudioFileId
        };
        return Task.FromResult(
            ServiceResult<SceneAudioAssignResponseDTO>.SuccessResult(response,
                MessageKey.Success_SceneAudioAssignment));
    }

    public Task<ServiceResult<List<SceneAudioFile>>> GetSceneAudioFilesAsync(SceneAudioGetDTO dto)
    {
        var list = new List<SceneAudioFile>
        {
            new()
            {
                SceneId = dto.SceneId,
                AudioFileId = Guid.NewGuid()
            }
        };
        return Task.FromResult(
            ServiceResult<List<SceneAudioFile>>.SuccessResult(list, MessageKey.Success_SceneAudioFilesRetrieval));
    }

    public Task<ServiceResult<bool>> RemoveSceneAudioFileAsync(SceneAudioRemoveDTO dto)
    {
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true, MessageKey.Success_SceneAudioRemoval));
    }

    public Task<ServiceResult<bool>> RemoveAllSceneAudioFilesAsync(SceneAudioRemoveAllDTO dto)
    {
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true, MessageKey.Success_AllSceneAudiosRemoval));
    }
}
public class FakeSceneValidationService : ISceneValidationService
{
    public Task<ServiceResult<bool>> ValidateSceneWithUserAsync(Guid sceneId, Guid userId)
    {
        if (sceneId == Guid.Empty)
            return Task.FromResult(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
    }
}

namespace TTT2.IntegrationTests
{
    public class SceneAudioServiceIntegrationTests
    {
        private readonly IAudioService _audioService = new FakeAudioService();
        private readonly IAuthenticationService _authenticationService = new FakeAuthenticationService();
        private readonly ISceneAudioServiceHelper _sceneAudioHelper = new FakeSceneAudioServiceHelper();
        private readonly SceneAudioService _sceneAudioService;
        private readonly ISceneValidationService _sceneValidationService = new FakeSceneValidationService();
        private readonly ClaimsPrincipal _testUser;
        private readonly Guid _testUserId = Guid.NewGuid();

        private readonly IUserClaimsService _userClaimsService = new FakeUserClaimsService();

        public SceneAudioServiceIntegrationTests()
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()));
            _testUser = new ClaimsPrincipal(identity);

            _sceneAudioService = new SceneAudioService(
                _userClaimsService,
                _audioService,
                _sceneAudioHelper,
                _authenticationService,
                _sceneValidationService);
        }

        [Fact]
        public async Task AssignAudio_Successful_Test()
        {
            // Arrange: create a SceneAudioAssignDTO with non-empty SceneId and AudioFileId.
            var assignDTO = new SceneAudioAssignDTO
            {
                SceneId = Guid.NewGuid(),
                AudioFileId = Guid.NewGuid()
            };

            // Act: Call AssignAudio.
            var result = await _sceneAudioService.AssignAudio(assignDTO, _testUser);

            // Assert: Verify the assignment succeeded and the returned data matches input.
            Assert.True(result.IsSuccess, "AssignAudio should succeed.");
            Assert.NotNull(result.Data);
            Assert.Equal(assignDTO.SceneId, result.Data.SceneId);
            Assert.Equal(assignDTO.AudioFileId, result.Data.AudioFileId);
        }

        [Fact]
        public async Task GetSceneAudioFilesBySceneIdAsync_Successful_Test()
        {
            // Arrange: create a SceneAudioGetDTO with a valid SceneId.
            var getDTO = new SceneAudioGetDTO
            {
                SceneId = Guid.NewGuid()
            };

            // Act: Call GetSceneAudioFilesBySceneIdAsync.
            var result = await _sceneAudioService.GetSceneAudioFilesBySceneIdAsync(getDTO, _testUser);

            // Assert: Verify retrieval succeeded and at least one scene audio file is returned.
            Assert.True(result.IsSuccess, "GetSceneAudioFilesBySceneIdAsync should succeed.");
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task RemoveAudio_Successful_Test()
        {
            // Arrange: create a SceneAudioRemoveDTO with valid SceneId and AudioFileId.
            var removeDTO = new SceneAudioRemoveDTO
            {
                SceneId = Guid.NewGuid(),
                AudioFileId = Guid.NewGuid()
            };

            // Act: Call RemoveAudio.
            var result = await _sceneAudioService.RemoveAudio(removeDTO, _testUser);

            // Assert: Verify removal succeeded.
            Assert.True(result.IsSuccess, "RemoveAudio should succeed.");
            Assert.True(result.Data);
        }

        [Fact]
        public async Task RemoveAllAudioForSceneAsync_Successful_Test()
        {
            // Arrange: create a SceneAudioRemoveAllDTO with a valid SceneId.
            var removeAllDTO = new SceneAudioRemoveAllDTO
            {
                SceneId = Guid.NewGuid()
            };

            // Act: Call RemoveAllAudioForSceneAsync.
            var result = await _sceneAudioService.RemoveAllAudioForSceneAsync(removeAllDTO, _testUser);

            // Assert: Verify that all audio files for the scene were removed successfully.
            Assert.True(result.IsSuccess, "RemoveAllAudioForSceneAsync should succeed.");
            Assert.True(result.Data);
        }
    }
}