using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Enums;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.AudioStreaming;
using Shared.Statics;
using TTT2.Tests.Factories;

namespace TTT2.Tests.Endpoint;

[Trait("Category", "Endpoint")]
public class AudioStreamingControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string AssetsFolder = "Assets";
    private const string TestFileName = "TestAudioFile.mp3";

    [Fact]
    public async Task StreamAudio_FullFile_Returns200AndCorrectBytes()
    {
        // 1) Register & login
        var token = await RegisterAndLoginAsync("streamuser1", "stream1@example.com", "Password123!");
        SetAuthorizationHeader(token);

        // 2) Upload an audio via the existing endpoint
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), AssetsFolder, TestFileName);
        File.Exists(filePath).Should().BeTrue("Test audio asset must be present for streaming tests.");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

        var form = new MultipartFormDataContent
        {
            { fileContent, "AudioFile", TestFileName },
            { new StringContent("Stream Test Audio"), "Name" }
        };

        var createResp = await _client.PostAsync("/audio/create-audio", form);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var createApiResp = await createResp.Content.ReadFromJsonAsync<ApiResponse<AudioFileCreateResponseDTO>>();
        createApiResp.Should().NotBeNull();
        var audioId = createApiResp!.Data.Id;

        // 3) Call the streaming endpoint (no Range header)
        var streamReq = new HttpRequestMessage(HttpMethod.Post, "/audio-streaming/stream")
        {
            Content = JsonContent.Create(new AudioStreamDTO { AudioId = audioId })
        };
        streamReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var streamResp = await _client.SendAsync(streamReq);
        streamResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4) Assert headers
        streamResp.Content.Headers.ContentType.MediaType
            .Should().Be("audio/mpeg");
        streamResp.Content.Headers.ContentLength
            .Should().Be(fileBytes.Length);
        streamResp.Headers.Contains("Accept-Ranges")
            .Should().BeTrue();
        streamResp.Headers.GetValues("Accept-Ranges")
            .Should().ContainSingle("bytes");

        // 5) Read and compare bytes
        var streamedBytes = await streamResp.Content.ReadAsByteArrayAsync();
        streamedBytes.Should().Equal(fileBytes);
    }

    [Fact]
    public async Task StreamAudio_PartialFile_Returns206AndSlice()
    {
        // 1) Register & login
        var token = await RegisterAndLoginAsync("streamuser2", "stream2@example.com", "Password123!");
        SetAuthorizationHeader(token);

        // 2) Upload same test file
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), AssetsFolder, TestFileName);
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        using var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

        var form = new MultipartFormDataContent
        {
            { fileContent, "AudioFile", TestFileName },
            { new StringContent("Partial Stream Audio"), "Name" }
        };

        var createResp = await _client.PostAsync("/audio/create-audio", form);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var createApiResp = await createResp.Content.ReadFromJsonAsync<ApiResponse<AudioFileCreateResponseDTO>>();
        var audioId = createApiResp!.Data.Id;

        // 3) Call streaming endpoint with Range header: bytes=0-3
        var sliceReq = new HttpRequestMessage(HttpMethod.Post, "/audio-streaming/stream")
        {
            Content = JsonContent.Create(new AudioStreamDTO { AudioId = audioId })
        };
        sliceReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        sliceReq.Headers.Add("Range", "bytes=0-3");

        var sliceResp = await _client.SendAsync(sliceReq);
        sliceResp.StatusCode.Should().Be(HttpStatusCode.PartialContent);

        // 4) Assert headers
        sliceResp.Content.Headers.ContentType?.MediaType
            .Should().Be("audio/mpeg");
        sliceResp.Content.Headers.ContentLength
            .Should().Be(4);
        sliceResp.Headers.Contains("Accept-Ranges")
            .Should().BeTrue();
        sliceResp.Headers.GetValues("Accept-Ranges")
            .Should().ContainSingle("bytes");
        sliceResp.Content.Headers.Contains("Content-Range")
            .Should().BeTrue();
        var expectedContentRange = $"bytes 0-3/{fileBytes.Length}";
        sliceResp.Content.Headers.GetValues("Content-Range")
            .Should().ContainSingle(expectedContentRange);

        // 5) Read slice and verify
        var sliceBytes = await sliceResp.Content.ReadAsByteArrayAsync();
        sliceBytes.Should().Equal(fileBytes[0..4]);
    }

    [Fact]
    public async Task StreamAudio_InvalidFile_ReturnsNotFoundJson()
    {
        // 1) Register & login
        var token = await RegisterAndLoginAsync("streamuser3", "stream3@example.com", "Password123!");
        SetAuthorizationHeader(token);

        // 2) Call streaming for a random GUID, no upload
        var badReq = new HttpRequestMessage(HttpMethod.Post, "/audio-streaming/stream")
        {
            Content = JsonContent.Create(new AudioStreamDTO { AudioId = Guid.NewGuid() })
        };
        badReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var badResp = await _client.SendAsync(badReq);

        // 3) Should return JSON error via your converter
        badResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await badResp.Content.ReadAsStringAsync();
        body.Should().Contain(MessageRepository.GetMessage(MessageKey.Error_Unauthorized).UserMessage);
    }
}