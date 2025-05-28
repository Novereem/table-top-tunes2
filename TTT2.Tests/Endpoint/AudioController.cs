using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using TTT2.Tests.Factories;

namespace TTT2.Tests.Endpoint;

[Trait("Category", "Endpoint")]
public class AudioController(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateAudio_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange: Obtain a valid token.
        const string username = "testuser";
        const string email = "testuser@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        // Arrange: Prepare audio creation using MultipartFormDataContent.
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

        // Prepare the form data for your upload
        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "AudioFile", "TestAudioFile.mp3");
        formData.Add(new StringContent("Test Audio"), "Name");
            
        // Act: Call the AudioController endpoint to create the audio file.
        var createResponse = await _client.PostAsync("/audio/create-audio", formData);
        
        // Assert: Expect the response to indicate the audio was created.
        var internalError = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Request failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalError}");
        var audioApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AudioFileCreateResponseDTO>>();
        audioApiResponse.Should().NotBeNull();
        audioApiResponse!.Data.Should().NotBeNull();
        audioApiResponse.Data.Name.Should().Be("Test Audio");
    }

    [Fact]
    public async Task RemoveAudio_ShouldReturnOkAndDeleteAudio()
    {
        // Arrange: Register and log in a new user.
        const string username = "removeaudiouser";
        const string email = "removeaudiouser@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        // Arrange: Use a valid audio file from the Assets folder.
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
        formData.Add(new StringContent("Remove Audio"), "Name");

        // Act: First, create the audio file.
        var createResponse = await _client.PostAsync("/audio/create-audio", formData);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AudioFileCreateResponseDTO>>();
        createApiResponse.Should().NotBeNull();
        var audioId = createApiResponse.Data.Id;
        audioId.Should().NotBeEmpty();

        // Arrange: Prepare the removal DTO.
        var removeDto = new AudioFileRemoveDTO { AudioId = audioId };

        // Act: Call DELETE /audio/remove-audio with the removal DTO.
        var request = new HttpRequestMessage(HttpMethod.Delete, "/audio/remove-audio")
        {
            Content = JsonContent.Create(removeDto)
        };
        var removeResponse = await _client.SendAsync(request);

        // Assert: Expect HTTP 200 OK and a success indicator.
        var internalErrorRemove = await ExtractInternalErrorAsync(removeResponse);
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await removeResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorRemove}");
        var removeApiResponse = await removeResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        removeApiResponse.Should().NotBeNull();
        removeApiResponse.Data.Should().BeTrue();
    }
}