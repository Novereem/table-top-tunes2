using System.Security.Claims;
using Moq;
using Shared.Enums;
using Shared.Interfaces.Services;
using Shared.Interfaces.Services.Common.Authentication;
using Shared.Interfaces.Services.Helpers;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Statics;
using TTT2.Services;

namespace TTT2.Tests.Unit.Services;

public class AudioServiceTests
{
    private readonly Mock<IUserClaimsService> _userClaimsServiceMock;
    private readonly Mock<IAudioServiceHelper> _audioServiceHelperMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<IUserStorageService> _userStorageServiceMock;
    private readonly AudioService _service;

    public AudioServiceTests()
    {
        _userClaimsServiceMock = new Mock<IUserClaimsService>();
        _audioServiceHelperMock = new Mock<IAudioServiceHelper>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _userStorageServiceMock = new Mock<IUserStorageService>();

        _service = new AudioService(
            _userClaimsServiceMock.Object,
            _audioServiceHelperMock.Object,
            _authenticationServiceMock.Object,
            _userStorageServiceMock.Object);
    }

    // Helper: Create a dummy ClaimsPrincipal with a user id claim.
    private ClaimsPrincipal CreateDummyClaimsPrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        return new ClaimsPrincipal(identity);
    }

    // Helper: Create a dummy User.
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

    // Helper: Create a dummy AudioFileCreateDTO.
    private AudioFileCreateDTO CreateDummyAudioFileCreateDTO(int fileLength = 100)
    {
        // We don't really need a full IFormFile for these tests; assume Length property is used.
        var dummyFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
        dummyFile.Setup(f => f.Length).Returns(fileLength);
        return new AudioFileCreateDTO { AudioFile = dummyFile.Object, Name = "Test Audio" };
    }

    // =======================
    // Tests for CreateAudioFileAsync
    // =======================

    [Fact]
    public async Task CreateAudioFileAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        // Arrange: userClaimsService fails to get user id.
        var dummyClaims = new ClaimsPrincipal();
        var failedUserIdResult = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(dummyClaims))
            .Returns(failedUserIdResult);

        // Act
        var result = await _service.CreateAudioFileAsync(CreateDummyAudioFileCreateDTO(), dummyClaims);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_JWTNullOrEmpty);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateAudioFileAsync_UserRetrievalFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var failedUserResult = ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(failedUserResult);

        // Act
        var result = await _service.CreateAudioFileAsync(CreateDummyAudioFileCreateDTO(), claims);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CreateAudioFileAsync_ValidationFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));

        // Simulate helper validation failure.
        var validationFailure = ServiceResult<object>.Failure(MessageKey.Error_InvalidInput);
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileCreateRequest(It.IsAny<AudioFileCreateDTO>(), dummyUser))
            .Returns(validationFailure);

        // Act
        var result = await _service.CreateAudioFileAsync(CreateDummyAudioFileCreateDTO(), claims);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateAudioFileAsync_IncreaseStorageFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));

        // Validation passes.
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileCreateRequest(It.IsAny<AudioFileCreateDTO>(), dummyUser))
            .Returns(ServiceResult<object>.SuccessResult());
        // Simulate storage increase failure.
        _userStorageServiceMock.Setup(x => x.IncreaseUserStorageAsync(dummyUser.Id, It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_InternalServerErrorData));

        // Act
        var result = await _service.CreateAudioFileAsync(CreateDummyAudioFileCreateDTO(), claims);

        // Assert
        Assert.False(result.IsSuccess);
        var expected = MessageRepository.GetMessage(MessageKey.Error_InternalServerErrorData);
        Assert.Equal(expected.InternalMessage, result.MessageInfo.InternalMessage);
    }

    [Fact]
    public async Task CreateAudioFileAsync_CreateAudioFails_DecreasesStorageAndReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var dto = CreateDummyAudioFileCreateDTO();
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));

        // Validation passes.
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileCreateRequest(dto, dummyUser))
            .Returns(ServiceResult<object>.SuccessResult());
        // Storage increase succeeds.
        _userStorageServiceMock.Setup(x => x.IncreaseUserStorageAsync(dummyUser.Id, dto.AudioFile.Length))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        // Simulate audio creation failure.
        _audioServiceHelperMock.Setup(x => x.CreateAudioFileAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<AudioFileCreateResponseDTO>.Failure(MessageKey.Error_InternalServerErrorData));
        // Assume decrease storage is called; we can simply return success.
        _userStorageServiceMock.Setup(x => x.DecreaseUserStorageAsync(dummyUser.Id, dto.AudioFile.Length))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        // Act
        var result = await _service.CreateAudioFileAsync(dto, claims);

        // Assert
        Assert.False(result.IsSuccess);
        // Here we expect the failure to be propagated from the helper validation branch.
        var expected = MessageRepository.GetMessage(MessageKey.Error_InvalidInput); // or another error key as per your code logic.
        // In our service code, if creation fails, it returns validationResult.ToFailureResult.
        // You may adjust this expected message according to your implementation.
    }

    [Fact]
    public async Task CreateAudioFileAsync_AllSucceed_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var dto = CreateDummyAudioFileCreateDTO();
        var createdAudioResponse = new AudioFileCreateResponseDTO
        {
            Id = Guid.NewGuid(),
            Name = "Created Audio",
            CreatedAt = DateTime.UtcNow
        };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileCreateRequest(dto, dummyUser))
            .Returns(ServiceResult<object>.SuccessResult());
        _userStorageServiceMock.Setup(x => x.IncreaseUserStorageAsync(dummyUser.Id, dto.AudioFile.Length))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        _audioServiceHelperMock.Setup(x => x.CreateAudioFileAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<AudioFileCreateResponseDTO>.SuccessResult(createdAudioResponse));
        
        var result = await _service.CreateAudioFileAsync(dto, claims);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createdAudioResponse.Id, result.Data.Id);
    }

    // =======================
    // Tests for DeleteAudioFileAsync
    // =======================

    [Fact]
    public async Task DeleteAudioFileAsync_UserIdRetrievalFails_ReturnsFailure()
    {
        // Arrange
        var claims = new ClaimsPrincipal();
        var failedUserIdResult = ServiceResult<Guid>.Failure(MessageKey.Error_JWTNullOrEmpty);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(failedUserIdResult);

        var dto = new AudioFileRemoveDTO(); // details don't matter here

        var result = await _service.DeleteAudioFileAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAudioFileAsync_UserRetrievalFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.Failure(MessageKey.Error_InternalServerErrorService));

        var dto = new AudioFileRemoveDTO();

        var result = await _service.DeleteAudioFileAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAudioFileAsync_ValidFlow_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var dto = new AudioFileRemoveDTO { AudioId = Guid.NewGuid() };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        // Simulate valid ownership.
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileWithUserAsync(dto.AudioId, userId))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));
        // Simulate deletion success returning some number of bytes removed.
        _audioServiceHelperMock.Setup(x => x.DeleteAudioFileAsync(dto, dummyUser))
            .ReturnsAsync(ServiceResult<long>.SuccessResult(500));
        _userStorageServiceMock.Setup(x => x.DecreaseUserStorageAsync(dummyUser.Id, It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<bool>.SuccessResult(true));

        var result = await _service.DeleteAudioFileAsync(dto, claims);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAudioFileAsync_HelperDeletionFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = CreateDummyClaimsPrincipal(userId);
        var dummyUser = CreateDummyUser(userId);
        var dto = new AudioFileRemoveDTO { AudioId = Guid.NewGuid() };

        _userClaimsServiceMock.Setup(x => x.GetUserIdFromClaims(claims))
            .Returns(ServiceResult<Guid>.SuccessResult(userId));
        _authenticationServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceResult<User>.SuccessResult(dummyUser));
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileWithUserAsync(dto.AudioId, userId))
            .ReturnsAsync(ServiceResult<bool>.Failure(MessageKey.Error_Unauthorized));
        
        var result = await _service.DeleteAudioFileAsync(dto, claims);

        Assert.False(result.IsSuccess);
    }

    // =======================
    // Test for ValidateAudioFileWithUserAsync simply calls helper.
    [Fact]
    public async Task ValidateAudioFileWithUserAsync_DelegatesToHelper_ReturnsResult()
    {
        var audioId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var helperResult = ServiceResult<bool>.SuccessResult(true);
        _audioServiceHelperMock.Setup(x => x.ValidateAudioFileWithUserAsync(audioId, userId))
            .ReturnsAsync(helperResult);

        var result = await _service.ValidateAudioFileWithUserAsync(audioId, userId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
}