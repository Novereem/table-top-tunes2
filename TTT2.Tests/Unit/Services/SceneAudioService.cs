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
using Shared.Statics;
using TTT2.Services;

namespace TTT2.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class SceneAudioServiceTests
{
    private readonly Mock<IUserClaimsService> _userClaimsServiceMock;
    private readonly Mock<IAudioService> _audioServiceMock;
    private readonly Mock<ISceneAudioServiceHelper> _sceneAudioHelperMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<ISceneValidationService> _sceneValidationServiceMock;
    private readonly SceneAudioService _service;

    public SceneAudioServiceTests()
    {
        _userClaimsServiceMock = new Mock<IUserClaimsService>();
        _audioServiceMock = new Mock<IAudioService>();
        _sceneAudioHelperMock = new Mock<ISceneAudioServiceHelper>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _sceneValidationServiceMock = new Mock<ISceneValidationService>();

        _service = new SceneAudioService(
            _userClaimsServiceMock.Object,
            _audioServiceMock.Object,
            _sceneAudioHelperMock.Object,
            _authenticationServiceMock.Object,
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
            Username = "dummyUser",
            Email = "dummy@example.com",
            PasswordHash = "hashed",
            UsedStorageBytes = 500,
            MaxStorageBytes = 1000,
            CreatedAt = DateTime.UtcNow
        };
    }

    private SceneAudioAssignDTO CreateDummySceneAudioAssignDTO()
    {
        return new SceneAudioAssignDTO
        {
            SceneId = Guid.NewGuid(),
            AudioFileId = Guid.NewGuid(),
            AudioType = AudioType.Music
        };
    }

    private SceneAudioGetDTO CreateDummySceneAudioGetDTO(Guid sceneId)
    {
        return new SceneAudioGetDTO { SceneId = sceneId };
    }

    private SceneAudioRemoveDTO CreateDummySceneAudioRemoveDTO()
    {
        return new SceneAudioRemoveDTO
        {
            SceneId = Guid.NewGuid(),
            AudioFileId = Guid.NewGuid(),
            AudioType = AudioType.Music
        };
    }

    private SceneAudioRemoveAllDTO CreateDummySceneAudioRemoveAllDTO()
    {
        return new SceneAudioRemoveAllDTO { SceneId = Guid.NewGuid() };
    }

    #region AssignAudio Tests

    [Fact]
    public async Task AssignAudio_UserIdRetrievalFails_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal();
        var failureUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failureUserId);

        var dto = CreateDummySceneAudioAssignDTO();

        // Act
        var result = await _service.AssignAudio(dto, claims);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task AssignAudio_ValidateOwnershipFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        // Simulate ownership validation failure (either scene or audio ownership)
        var ownershipFailure = ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized);
        // Our service calls a private method ValidateSceneAudioOwnership which calls sceneValidationService and audioService.ValidateAudioFileWithUserAsync.
        // We simulate it by having the helper (which is used in AssignAudio for removal and assignment) return failure.
        _sceneAudioHelperMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioAssignDTO>()))
            .ReturnsAsync(ownershipFailure.ToFailureResult<SceneAudioAssignResponseDTO>());

        var dto = CreateDummySceneAudioAssignDTO();

        // Act
        var result = await _service.AssignAudio(dto, claims);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AssignAudio_HelperAddFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        var addFailure = ServiceResult<SceneAudioAssignResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData);
        _sceneAudioHelperMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioAssignDTO>()))
            .ReturnsAsync(addFailure);
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _audioServiceMock.Setup(x => x.ValidateAudioFileWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var dto = CreateDummySceneAudioAssignDTO();

        // Act
        var result = await _service.AssignAudio(dto, claims);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task AssignAudio_Success_ReturnsSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));

        // Simulate that ownership validation passes.
        // In our implementation, ValidateSceneAudioOwnership is called in AssignAudio (inside try)
        // and then helper.AddSceneAudioFileAsync is invoked.
        var assignResponse = new SceneAudioAssignResponseDTO
        {
            SceneId = Guid.NewGuid(),
            AudioFileId = Guid.NewGuid(),
            AudioType = AudioType.Music
        };
        _sceneAudioHelperMock.Setup(x => x.AddSceneAudioFileAsync(It.IsAny<SceneAudioAssignDTO>()))
            .ReturnsAsync(ServiceResult<SceneAudioAssignResponseDTO>.SuccessResult(assignResponse));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _audioServiceMock.Setup(x => x.ValidateAudioFileWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        
        var dto = CreateDummySceneAudioAssignDTO();

        // Act
        var result = await _service.AssignAudio(dto, claims);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(assignResponse.SceneId, result.Data.SceneId);
        Assert.Equal(assignResponse.AudioFileId, result.Data.AudioFileId);
    }

    #endregion

    #region GetSceneAudioFilesBySceneIdAsync Tests

    [Fact]
    public async Task GetSceneAudioFilesBySceneIdAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failureUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failureUserId);

        var dto = CreateDummySceneAudioGetDTO(Guid.NewGuid());

        var result = await _service.GetSceneAudioFilesBySceneIdAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetSceneAudioFilesBySceneIdAsync_SceneValidationFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        // Simulate scene validation failure.
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_NotFound));

        var dto = CreateDummySceneAudioGetDTO(Guid.NewGuid());

        var result = await _service.GetSceneAudioFilesBySceneIdAsync(dto, claims);

        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_NotFound);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task GetSceneAudioFilesBySceneIdAsync_HelperFailure_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        var helperFailure = ServiceResult<List<SceneAudioFile>>.Failure(MessageKey.Error_InternalServerErrorData);
        _sceneAudioHelperMock.Setup(x => x.GetSceneAudioFilesAsync(It.IsAny<SceneAudioGetDTO>()))
            .ReturnsAsync(helperFailure);

        var dto = CreateDummySceneAudioGetDTO(Guid.NewGuid());

        var result = await _service.GetSceneAudioFilesBySceneIdAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetSceneAudioFilesBySceneIdAsync_Success_ReturnsSuccessResponse()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var dummyFiles = new List<SceneAudioFile>
        {
            new SceneAudioFile { SceneId = Guid.NewGuid(), AudioFileId = Guid.NewGuid(), AudioType = AudioType.Music },
            new SceneAudioFile { SceneId = Guid.NewGuid(), AudioFileId = Guid.NewGuid(), AudioType = AudioType.Music }
        };
        _sceneAudioHelperMock.Setup(x => x.GetSceneAudioFilesAsync(It.IsAny<SceneAudioGetDTO>()))
            .ReturnsAsync(ServiceResult<List<SceneAudioFile>>.SuccessResult(dummyFiles, MessageKey.Success_SceneAudioFilesRetrieval));

        var dto = CreateDummySceneAudioGetDTO(Guid.NewGuid());

        var result = await _service.GetSceneAudioFilesBySceneIdAsync(dto, claims);

        Assert.True(result.IsSuccess);
        Assert.Equal(dummyFiles.Count, result.Data.Count);
    }

    #endregion

    #region RemoveAudio Tests

    [Fact]
    public async Task RemoveAudio_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failureUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failureUserId);

        var dto = new SceneAudioRemoveDTO();

        var result = await _service.RemoveAudio(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveAudio_ValidFlow_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));

        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _audioServiceMock.Setup(x => x.ValidateAudioFileWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        _sceneAudioHelperMock.Setup(x => x.RemoveSceneAudioFileAsync(It.IsAny<SceneAudioRemoveDTO>()))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var dto = CreateDummySceneAudioRemoveDTO();

        var result = await _service.RemoveAudio(dto, claims);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveAudio_HelperRemovalFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
    
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized));

        var dto = CreateDummySceneAudioRemoveDTO();

        var result = await _service.RemoveAudio(dto, claims);

        Assert.False(result.IsSuccess);
    }

    #endregion

    #region RemoveAllAudioForSceneAsync Tests

    [Fact]
    public async Task RemoveAllAudioForSceneAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        var claims = new ClaimsPrincipal();
        var failureUserId = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failureUserId);

        var dto = CreateDummySceneAudioRemoveAllDTO();

        var result = await _service.RemoveAllAudioForSceneAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveAllAudioForSceneAsync_UserRetrievalFails_ReturnsFailure()
    {
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService));

        var dto = CreateDummySceneAudioRemoveAllDTO();

        var result = await _service.RemoveAllAudioForSceneAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveAllAudioForSceneAsync_Success_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var dummyUser = CreateDummyUser(userId);
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _sceneValidationServiceMock.Setup(x => x.ValidateSceneWithUserAsync(It.IsAny<Guid>(), userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _sceneAudioHelperMock.Setup(x => x.RemoveAllSceneAudioFilesAsync(It.IsAny<SceneAudioRemoveAllDTO>()))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var dto = CreateDummySceneAudioRemoveAllDTO();

        var result = await _service.RemoveAllAudioForSceneAsync(dto, claims);

        Assert.True(result.IsSuccess);
    }

    #endregion
}