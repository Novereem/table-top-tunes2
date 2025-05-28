using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.SceneAudios;
using Shared.Models.DTOs.Scenes;
using TTT2.Tests.Factories;

namespace TTT2.Tests.Endpoint;

[Trait("Category", "Endpoint")]
public class SceneAudioController(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<Guid> CreateSceneAsync(string sceneName)
    {
        var createDto = new SceneCreateDTO { Name = sceneName };
        var createResponse = await _client.PostAsJsonAsync("/scenes/create-scene", createDto);

        var internalErrorCreate = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Scene creation failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorCreate}");

        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<SceneCreateResponseDTO>>();
        createApiResponse.Should().NotBeNull();
        var sceneId = createApiResponse!.Data.Id;
        sceneId.Should().NotBeEmpty();
        return sceneId;
    }
    
    private async Task<Guid> CreateAudioFileAsync(string audioName = "Test Audio")
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var assetsFolder = Path.Combine(currentDirectory, "Assets");
        var filePath = Path.Combine(assetsFolder, "TestAudioFile.mp3");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Test audio file not found.", filePath);
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "AudioFile", "TestAudioFile.mp3");
        formData.Add(new StringContent(audioName), "Name");

        var createResponse = await _client.PostAsync("/audio/create-audio", formData);
        var internalErrorAudio = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Audio creation failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorAudio}");

        var apiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AudioFileCreateResponseDTO>>();
        apiResponse.Should().NotBeNull();
        return apiResponse!.Data.Id;
    }

    [Fact]
    public async Task AssignAudio_ShouldReturnSuccess()
    {
        // Arrange: Register, log in, create a scene and an audio file.
        const string username = "sceneAudioUser";
        const string email = "sceneAudioUser@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        var sceneId = await CreateSceneAsync("Scene for Audio Assignment");
        var audioFileId = await CreateAudioFileAsync();

        // Build assignment DTO using the actual scene and audio file.
        var assignDto = new SceneAudioAssignDTO
        {
            SceneId = sceneId,
            AudioFileId = audioFileId,
            AudioType = AudioType.Music
        };

        // Act: Call the assign-audio endpoint.
        var assignResponse = await _client.PostAsJsonAsync("/sceneaudio/assign-audio", assignDto);

        // Assert: Check status code with internal error details.
        var internalError = await ExtractInternalErrorAsync(assignResponse);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await assignResponse.Content.ReadAsStringAsync()} | Internal Error: {internalError}");

        var apiResponse = await assignResponse.Content.ReadFromJsonAsync<ApiResponse<SceneAudioAssignResponseDTO>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data.SceneId.Should().Be(assignDto.SceneId);
        apiResponse.Data.AudioFileId.Should().Be(assignDto.AudioFileId);
        apiResponse.Data.AudioType.Should().Be(assignDto.AudioType);
    }

    [Fact]
    public async Task RemoveAudio_ShouldReturnSuccess()
    {
        // Arrange: Register, log in, create a scene and an audio file.
        const string username = "sceneAudioUser2";
        const string email = "sceneAudioUser2@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        var sceneId = await CreateSceneAsync("Scene for Audio Removal");
        var audioFileId = await CreateAudioFileAsync();

        // First, assign an audio file to the scene.
        var assignDto = new SceneAudioAssignDTO
        {
            SceneId = sceneId,
            AudioFileId = audioFileId,
            AudioType = AudioType.Music
        };

        var assignResponse = await _client.PostAsJsonAsync("/sceneaudio/assign-audio", assignDto);
        var internalErrorAssign = await ExtractInternalErrorAsync(assignResponse);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Audio assignment failed. Raw response: {await assignResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorAssign}");

        // Act: Remove the audio file from the scene.
        var removeDto = new SceneAudioRemoveDTO
        {
            SceneId = assignDto.SceneId,
            AudioFileId = assignDto.AudioFileId,
            AudioType = assignDto.AudioType
        };

        var removeRequest = new HttpRequestMessage(HttpMethod.Delete, "/sceneaudio/remove-audio")
        {
            Content = JsonContent.Create(removeDto)
        };
        var removeResponse = await _client.SendAsync(removeRequest);

        // Assert: Check removal response with internal error details.
        var internalErrorRemove = await ExtractInternalErrorAsync(removeResponse);
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await removeResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorRemove}");

        var removeApiResponse = await removeResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        removeApiResponse.Should().NotBeNull();
        removeApiResponse!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAllAudio_ShouldReturnSuccess()
    {
        // Arrange: Register, log in, create a scene and audio file.
        const string username = "sceneAudioUser3";
        const string email = "sceneAudioUser3@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        var sceneId = await CreateSceneAsync("Scene for Remove All Audio");
        // Create two audio files.
        var audioFileId1 = await CreateAudioFileAsync("Audio 1");
        var audioFileId2 = await CreateAudioFileAsync("Audio 2");

        // Assign two audio files to that scene.
        var assignDto1 = new SceneAudioAssignDTO
        {
            SceneId = sceneId,
            AudioFileId = audioFileId1,
            AudioType = AudioType.Music
        };
        var assignDto2 = new SceneAudioAssignDTO
        {
            SceneId = sceneId,
            AudioFileId = audioFileId2,
            AudioType = AudioType.Ambient
        };

        var assignResp1 = await _client.PostAsJsonAsync("/sceneaudio/assign-audio", assignDto1);
        var internalError1 = await ExtractInternalErrorAsync(assignResp1);
        assignResp1.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Assign audio 1 failed. Raw response: {await assignResp1.Content.ReadAsStringAsync()} | Internal Error: {internalError1}");

        var assignResp2 = await _client.PostAsJsonAsync("/sceneaudio/assign-audio", assignDto2);
        var internalError2 = await ExtractInternalErrorAsync(assignResp2);
        assignResp2.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Assign audio 2 failed. Raw response: {await assignResp2.Content.ReadAsStringAsync()} | Internal Error: {internalError2}");

        // Act: Call the remove-all-audio endpoint.
        var removeAllDto = new SceneAudioRemoveAllDTO { SceneId = sceneId };
        var removeAllRequest = new HttpRequestMessage(HttpMethod.Delete, "/sceneaudio/remove-all-audio")
        {
            Content = JsonContent.Create(removeAllDto)
        };
        var removeAllResponse = await _client.SendAsync(removeAllRequest);

        // Assert: Check status and removal result.
        var internalErrorRemoveAll = await ExtractInternalErrorAsync(removeAllResponse);
        removeAllResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await removeAllResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorRemoveAll}");

        var removeAllApiResponse = await removeAllResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        removeAllApiResponse.Should().NotBeNull();
        removeAllApiResponse!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task GetSceneAudio_ShouldReturnAudioFiles()
    {
        // Arrange: Register, log in, create a scene and two audio files.
        const string username = "sceneAudioUser4";
        const string email = "sceneAudioUser4@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        var sceneId = await CreateSceneAsync("Scene for Get Audio");
        var audioFileId1 = await CreateAudioFileAsync("Audio 1");
        var audioFileId2 = await CreateAudioFileAsync("Audio 2");

        // Assign two audio files to the scene.
        var assignDto1 = new SceneAudioAssignDTO
        {
            SceneId = sceneId,
            AudioFileId = audioFileId1,
            AudioType = AudioType.Music
        };
        var assignDto2 = new SceneAudioAssignDTO
        {
            SceneId = sceneId,
            AudioFileId = audioFileId2,
            AudioType = AudioType.Ambient
        };

        var assignResp1 = await _client.PostAsJsonAsync("/sceneaudio/assign-audio", assignDto1);
        var internalError1 = await ExtractInternalErrorAsync(assignResp1);
        assignResp1.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Assign audio 1 failed. Raw response: {await assignResp1.Content.ReadAsStringAsync()} | Internal Error: {internalError1}");

        var assignResp2 = await _client.PostAsJsonAsync("/sceneaudio/assign-audio", assignDto2);
        var internalError2 = await ExtractInternalErrorAsync(assignResp2);
        assignResp2.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Assign audio 2 failed. Raw response: {await assignResp2.Content.ReadAsStringAsync()} | Internal Error: {internalError2}");

        // Act: Call the get-scene-audio endpoint.
        var getDto = new SceneAudioGetDTO { SceneId = sceneId };
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/sceneaudio/get-scene-audio")
        {
            Content = JsonContent.Create(getDto)
        };
        var getResponse = await _client.SendAsync(getRequest);

        // Assert: Validate the GET response.
        var internalErrorGet = await ExtractInternalErrorAsync(getResponse);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await getResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorGet}");

        var getApiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<SceneAudioFile>>>();
        getApiResponse.Should().NotBeNull();
        getApiResponse!.Data.Should().NotBeNull();
        getApiResponse.Data.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}