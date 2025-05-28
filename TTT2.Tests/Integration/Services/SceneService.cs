using System.Security.Claims;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.DTOs.Scenes;
using TTT2.Services;

[Trait("Category", "Integration")]
public class FakeSceneServiceHelper : ISceneServiceHelper
{
    public ServiceResult<object> ValidateSceneCreate(SceneCreateDTO sceneDTO)
    {
        if (string.IsNullOrWhiteSpace(sceneDTO.Name))
            return ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
        return ServiceResult<object>.SuccessResult(null);
    }

    public Task<ServiceResult<SceneCreateResponseDTO>> CreateSceneAsync(SceneCreateDTO sceneDTO, User user)
    {
        var response = new SceneCreateResponseDTO
        {
            Id = Guid.NewGuid(),
            Name = sceneDTO.Name,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<SceneCreateResponseDTO>.SuccessResult(response, MessageKey.Success_SceneCreation));
    }

    public Task<ServiceResult<List<Scene>>> RetrieveScenesByUserIdAsync(User user)
    {
        var scenes = new List<Scene>
        {
            new Scene
            {
                Id = Guid.NewGuid(),
                Name = "Test Scene",
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            }
        };
        return Task.FromResult(ServiceResult<List<Scene>>.SuccessResult(scenes, MessageKey.Success_DataRetrieved));
    }

    public Task<ServiceResult<SceneGetResponseDTO>> RetrieveSceneBySceneIdAsync(SceneGetDTO sceneGetDTO)
    {
        var response = new SceneGetResponseDTO
        {
            Id = sceneGetDTO.SceneId,
            Name = "Retrieved Scene",
            UserId = Guid.NewGuid(), // in fake, may not match but not used in test assertions
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<SceneGetResponseDTO>.SuccessResult(response, MessageKey.Success_DataRetrieved));
    }

    public ServiceResult<object> ValidateSceneUpdate(SceneUpdateDTO sceneUpdateDTO)
    {
        if (string.IsNullOrWhiteSpace(sceneUpdateDTO.NewName))
            return ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
        return ServiceResult<object>.SuccessResult(null);
    }

    public Task<ServiceResult<SceneUpdateResponseDTO>> UpdateSceneAsync(SceneUpdateDTO sceneUpdateDTO, User user)
    {
        var response = new SceneUpdateResponseDTO
        {
            SceneId = sceneUpdateDTO.SceneId,
            UpdatedName = sceneUpdateDTO.NewName,
            CreatedAt = DateTime.UtcNow
        };
        return Task.FromResult(ServiceResult<SceneUpdateResponseDTO>.SuccessResult(response, MessageKey.Success_SceneUpdate));
    }

    public Task<ServiceResult<bool>> DeleteSceneAsync(SceneRemoveDTO sceneRemoveDTO, User user)
    {
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true, MessageKey.Success_SceneRemoval));
    }
}

public class FakeSceneAudioService : ISceneAudioService
{
    public Task<HttpServiceResult<SceneAudioAssignResponseDTO>> AssignAudio(SceneAudioAssignDTO sceneAudioAssignDTO, ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }

    public Task<HttpServiceResult<bool>> RemoveAudio(SceneAudioRemoveDTO sceneAudioRemoveDTO, ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }

    public Task<HttpServiceResult<List<SceneAudioFile>>> GetSceneAudioFilesBySceneIdAsync(SceneAudioGetDTO sceneAudioGetDTO, ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }

    public Task<HttpServiceResult<bool>> RemoveAllAudioForSceneAsync(Shared.Models.DTOs.SceneAudios.SceneAudioRemoveAllDTO sceneAudioRemoveAllDTO, ClaimsPrincipal user)
    {
        return Task.FromResult(HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.SuccessResult(true, MessageKey.Success_AllSceneAudiosRemoval)));
    }
}

public class FakeSceneValidationServiceSceneService : ISceneValidationService
{
    public Task<ServiceResult<bool>> ValidateSceneWithUserAsync(Guid sceneId, Guid userId)
    {
        if (sceneId == Guid.Empty)
            return Task.FromResult(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true));
    }
}

// Integration tests for SceneService

namespace TTT2.Tests.IntegrationTests
{
    public class SceneServiceIntegrationTests
    {
        private readonly SceneService _sceneService;
        private readonly ClaimsPrincipal _testUser;
        private readonly Guid _testUserId = Guid.NewGuid();

        private readonly ISceneServiceHelper _sceneHelper = new FakeSceneServiceHelper();
        private readonly IUserClaimsService _userClaimsService = new FakeUserClaimsService();
        private readonly IAuthenticationService _authenticationService = new FakeAuthenticationService();
        private readonly ISceneAudioService _sceneAudioService = new FakeSceneAudioService();
        private readonly ISceneValidationService _sceneValidationService = new FakeSceneValidationService();

        public SceneServiceIntegrationTests()
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()));
            _testUser = new ClaimsPrincipal(identity);
            _sceneService = new SceneService(_sceneHelper, _userClaimsService, _authenticationService, _sceneAudioService, _sceneValidationService);
        }

        [Fact]
        public async Task CreateSceneAsync_Successful_Test()
        {
            // Arrange
            var createDTO = new SceneCreateDTO { Name = "New Scene" };

            // Act
            var result = await _sceneService.CreateSceneAsync(createDTO, _testUser);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("New Scene", result.Data.Name);
        }

        [Fact]
        public async Task GetScenesListByUserIdAsync_Successful_Test()
        {
            // Arrange

            // Act
            var result = await _sceneService.GetScenesListByUserIdAsync(_testUser);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetSceneByIdAsync_Successful_Test()
        {
            // Arrange
            var sceneId = Guid.NewGuid();
            var getDTO = new SceneGetDTO { SceneId = sceneId };

            // Act
            var result = await _sceneService.GetSceneByIdAsync(getDTO, _testUser);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(sceneId, result.Data.Id);
        }

        [Fact]
        public async Task UpdateSceneAsync_Successful_Test()
        {
            // Arrange
            var updateDTO = new SceneUpdateDTO { SceneId = Guid.NewGuid(), NewName = "Updated Scene" };

            // Act
            var result = await _sceneService.UpdateSceneAsync(updateDTO, _testUser);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated Scene", result.Data.UpdatedName);
        }

        [Fact]
        public async Task DeleteSceneAsync_Successful_Test()
        {
            // Arrange
            var removeDTO = new SceneRemoveDTO { SceneId = Guid.NewGuid() };

            // Act
            var result = await _sceneService.DeleteSceneAsync(removeDTO, _testUser);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
        }
    }
}
