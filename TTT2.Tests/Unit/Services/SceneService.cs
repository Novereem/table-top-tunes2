using System.Security.Claims;
using Moq;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Interfaces.Services.Helpers.Shared;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.DTOs.Scenes;
using Shared.Statics;
using TTT2.Services;

namespace TTT2.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class SceneServiceTests
{
    private readonly Mock<ISceneServiceHelper> _helperMock;
    private readonly Mock<IUserClaimsService> _userClaimsServiceMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<ISceneAudioService> _sceneAudioServiceMock;
    private readonly Mock<ISceneValidationService> _sceneValidationServiceMock;
    private readonly SceneService _service;

    public SceneServiceTests()
    {
        _helperMock = new Mock<ISceneServiceHelper>();
        _userClaimsServiceMock = new Mock<IUserClaimsService>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _sceneAudioServiceMock = new Mock<ISceneAudioService>();
        _sceneValidationServiceMock = new Mock<ISceneValidationService>();

        _service = new SceneService(
            _helperMock.Object,
            _userClaimsServiceMock.Object,
            _authenticationServiceMock.Object,
            _sceneAudioServiceMock.Object,
            _sceneValidationServiceMock.Object);
    }

    // Helpers for dummy DTOs, models, and ClaimsPrincipal.
    private ClaimsPrincipal CreateDummyClaimsPrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        return new ClaimsPrincipal(identity);
    }

    private User CreateDummyUser(Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            UsedStorageBytes = 0,
            MaxStorageBytes = 10 * 1024 * 1024,
            CreatedAt = DateTime.UtcNow
        };
    }

    private SceneCreateDTO CreateDummySceneCreateDTO(string name = "Test Scene")
    {
        return new SceneCreateDTO { Name = name };
    }

    private SceneRemoveDTO CreateDummySceneRemoveDTO(Guid sceneId)
    {
        return new SceneRemoveDTO { SceneId = sceneId };
    }

    private SceneGetDTO CreateDummySceneGetDTO(Guid sceneId)
    {
        return new SceneGetDTO { SceneId = sceneId };
    }

    private SceneUpdateDTO CreateDummySceneUpdateDTO(Guid sceneId, string newName = "Updated Scene")
    {
        return new SceneUpdateDTO { SceneId = sceneId, NewName = newName };
    }

    // For GetScenesListByUserIdAsync, assume helper.RetrieveScenesByUserIdAsync returns a List<Scene>.
    private List<Scene> CreateDummyScenesList(Guid userId, int count = 2)
    {
        var list = new List<Scene>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new Scene
            {
                Id = Guid.NewGuid(),
                Name = $"Scene {i + 1}",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        return list;
    }

    #region CreateSceneAsync Tests

    [Fact]
    public async Task CreateSceneAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failedUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failedUserId);

        var dto = CreateDummySceneCreateDTO();

        var result = await _service.CreateSceneAsync(dto, claims);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateSceneAsync_UserRetrievalFails_ReturnsFailure2()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var failedUser = ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(failedUser);

        var dto = CreateDummySceneCreateDTO();

        var result = await _service.CreateSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateSceneAsync_ValidationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));

        var validationFailure = ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
        _helperMock.Setup(x => x.ValidateSceneCreate(It.IsAny<SceneCreateDTO>()))
            .Returns(validationFailure);

        var dto = CreateDummySceneCreateDTO("");

        var result = await _service.CreateSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateSceneAsync_HelperCreationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));

        // Validation passes.
        _helperMock.Setup(x => x.ValidateSceneCreate(It.IsAny<SceneCreateDTO>()))
            .Returns(ServiceResult<object>.SuccessResult());
        // Simulate creation failure.
        var creationFailure = ServiceResult<SceneCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData);
        _helperMock.Setup(x => x.CreateSceneAsync(It.IsAny<SceneCreateDTO>(), dummyUser))
            .ReturnsAsync(creationFailure);

        var dto = CreateDummySceneCreateDTO("New Scene");

        var result = await _service.CreateSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateSceneAsync_Success_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var dto = CreateDummySceneCreateDTO("New Scene");
        var sceneResponse = new SceneCreateResponseDTO
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _helperMock.Setup(x => x.ValidateSceneCreate(dto))
            .Returns(ServiceResult<object>.SuccessResult());
        _helperMock.Setup(x => x.CreateSceneAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<SceneCreateResponseDTO>.SuccessResult(sceneResponse));

        var result = await _service.CreateSceneAsync(dto, claims);
        Assert.True(result.IsSuccess);
        Assert.Equal(sceneResponse.Id, result.Data.Id);
        Assert.Equal(sceneResponse.Name, result.Data.Name);
    }

    #endregion

    #region GetScenesListByUserIdAsync Tests

    [Fact]
    public async Task GetScenesListByUserIdAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failedUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failedUserId);

        var result = await _service.GetScenesListByUserIdAsync(claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetScenesListByUserIdAsync_UserRetrievalFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var failedUser = ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(failedUser);

        var result = await _service.GetScenesListByUserIdAsync(claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetScenesListByUserIdAsync_Success_ReturnsSceneList()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        var scenes = CreateDummyScenesList(userId, 3);

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _helperMock.Setup(x => x.RetrieveScenesByUserIdAsync(dummyUser))
            .ReturnsAsync(ServiceResult<List<Scene>>.SuccessResult(scenes));

        var result = await _service.GetScenesListByUserIdAsync(claims);
        Assert.True(result.IsSuccess);
        Assert.Equal(scenes.Count, result.Data.Count);
    }

    #endregion

    #region GetSceneByIdAsync Tests

    [Fact]
    public async Task GetSceneByIdAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failedUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failedUserId);

        var dto = CreateDummySceneGetDTO(Guid.NewGuid());
        var result = await _service.GetSceneByIdAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetSceneByIdAsync_SceneValidationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));

        var dto = CreateDummySceneGetDTO(Guid.NewGuid());
        var result = await _service.GetSceneByIdAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetSceneByIdAsync_Success_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        var sceneId = Guid.NewGuid();
        var dto = CreateDummySceneGetDTO(sceneId);
        var sceneGetResponse = new SceneGetResponseDTO
        {
            Id = sceneId,
            Name = "Retrieved Scene",
            UserId = dummyUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(sceneId, dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _helperMock.Setup(x => x.RetrieveSceneBySceneIdAsync(dto))
            .ReturnsAsync(ServiceResult<SceneGetResponseDTO>.SuccessResult(sceneGetResponse));

        var result = await _service.GetSceneByIdAsync(dto, claims);
        Assert.True(result.IsSuccess);
        Assert.Equal(sceneGetResponse.Id, result.Data.Id);
    }

    #endregion

    #region UpdateSceneAsync Tests

    [Fact]
    public async Task UpdateSceneAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failedUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failedUserId);

        var dto = CreateDummySceneUpdateDTO(Guid.NewGuid(), "Updated Scene");
        var result = await _service.UpdateSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateSceneAsync_ValidationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));

        var dto = CreateDummySceneUpdateDTO(Guid.NewGuid(), "Updated Scene");
        var result = await _service.UpdateSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateSceneAsync_HelperUpdateFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var dto = CreateDummySceneUpdateDTO(Guid.NewGuid(), "Updated Scene");

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(dto.SceneId, dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _helperMock.Setup(x => x.ValidateSceneUpdate(dto))
            .Returns(ServiceResult<object>.SuccessResult());
        // Simulate update failure.
        _helperMock.Setup(x => x.UpdateSceneAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<SceneUpdateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData));

        var result = await _service.UpdateSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateSceneAsync_Success_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        var dto = CreateDummySceneUpdateDTO(Guid.NewGuid(), "New Scene Name");

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(dto.SceneId, dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _helperMock.Setup(x => x.ValidateSceneUpdate(dto))
            .Returns(ServiceResult<object>.SuccessResult());
        var updateResponse = new SceneUpdateResponseDTO
        {
            SceneId = dto.SceneId,
            UpdatedName = dto.NewName,
            CreatedAt = DateTime.UtcNow
        };
        _helperMock.Setup(x => x.UpdateSceneAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<SceneUpdateResponseDTO>.SuccessResult(updateResponse));

        var result = await _service.UpdateSceneAsync(dto, claims);
        Assert.True(result.IsSuccess);
        Assert.Equal(updateResponse.UpdatedName, result.Data.UpdatedName);
    }

    #endregion

    #region DeleteSceneAsync Tests

    [Fact]
    public async Task DeleteSceneAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failedUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failedUserId);

        var dto = new SceneRemoveDTO { SceneId = Guid.NewGuid() };
        var result = await _service.DeleteSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteSceneAsync_UserRetrievalFails_ReturnsFailure2()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var failedUser = ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(failedUser);

        var dto = new SceneRemoveDTO { SceneId = Guid.NewGuid() };
        var result = await _service.DeleteSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteSceneAsync_ValidationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));

        var dto = new SceneRemoveDTO { SceneId = Guid.NewGuid() };
        var result = await _service.DeleteSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteSceneAsync_HelperDeletionFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        var dto = new SceneRemoveDTO { SceneId = Guid.NewGuid() };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(dto.SceneId, dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        // Simulate sceneAudioService removal (which is called to remove audio) succeeds.
        _sceneAudioServiceMock
            .Setup(x => x.RemoveAllAudioForSceneAsync(It.IsAny<SceneAudioRemoveAllDTO>(), claims))
            .ReturnsAsync(HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.SuccessResult(true)));

        // Simulate helper.DeleteSceneAsync failure.
        _helperMock.Setup(x => x.DeleteSceneAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData));

        var result = await _service.DeleteSceneAsync(dto, claims);
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task DeleteSceneAsync_Success_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        var dto = new SceneRemoveDTO { SceneId = Guid.NewGuid() };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(dto.SceneId, dummyUser.Id))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        // Simulate audio removal succeeds.
        _sceneAudioServiceMock
            .Setup(x => x.RemoveAllAudioForSceneAsync(It.IsAny<SceneAudioRemoveAllDTO>(), claims))
            .ReturnsAsync(HttpServiceResult<bool>.FromServiceResult(ServiceResult<bool>.SuccessResult(true)));

        // Simulate scene deletion succeeds.
        _helperMock.Setup(x => x.DeleteSceneAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var result = await _service.DeleteSceneAsync(dto, claims);
        Assert.True(result.IsSuccess);
    }

    #endregion
}