using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Shared.Models.Common;
using Shared.Models.DTOs.Scenes;
using TTT2.Tests.Factories;

namespace TTT2.Tests.Endpoint;

[Trait("Category", "Endpoint")]
public class SceneController(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateScene_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange: Register and log in to obtain a valid token.
        const string username = "sceneuser_create";
        const string email = "sceneuser_create@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        var createDto = new SceneCreateDTO { Name = "Test Scene" };

        // Act: Call the scene creation endpoint.
        var createResponse = await _client.PostAsJsonAsync("/scenes/create-scene", createDto);

        // Assert: Ensure the response status code is Created.
        var internalErrorCreate = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Request failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorCreate}");

        var apiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<SceneCreateResponseDTO>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data.Name.Should().Be("Test Scene");
        apiResponse.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSceneById_ShouldReturnScene()
    {
        // Arrange: Register, log in and create a scene.
        const string username = "sceneuser_get";
        const string email = "sceneuser_get@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        // Create a scene first.
        var createDto = new SceneCreateDTO { Name = "Scene for GetById" };
        var createResponse = await _client.PostAsJsonAsync("/scenes/create-scene", createDto);
        var internalErrorCreate = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Scene creation failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorCreate}");

        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<SceneCreateResponseDTO>>();
        createApiResponse.Should().NotBeNull();
        var sceneId = createApiResponse!.Data.Id;
        sceneId.Should().NotBeEmpty();

        // Arrange: Prepare the get DTO.
        var getDto = new SceneGetDTO { SceneId = sceneId };

        // Act: Send a GET request with a JSON body.
        var request = new HttpRequestMessage(HttpMethod.Get, "/scenes/get-scene")
        {
            Content = JsonContent.Create(getDto)
        };
        var getResponse = await _client.SendAsync(request);

        // Assert: Ensure the scene was returned successfully.
        var internalErrorGet = await ExtractInternalErrorAsync(getResponse);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await getResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorGet}");

        var getApiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<SceneGetResponseDTO>>();
        getApiResponse.Should().NotBeNull();
        getApiResponse!.Data.Should().NotBeNull();
        getApiResponse.Data.Id.Should().Be(sceneId);
        getApiResponse.Data.Name.Should().Be("Scene for GetById");
    }

    [Fact]
    public async Task GetScenesList_ShouldReturnListOfScenes()
    {
        // Arrange: Register, log in and create at least one scene.
        const string username = "sceneuser_list";
        const string email = "sceneuser_list@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        // Create a scene.
        var createDto = new SceneCreateDTO { Name = "Scene in List" };
        var createResponse = await _client.PostAsJsonAsync("/scenes/create-scene", createDto);
        var internalErrorCreate = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Scene creation failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorCreate}");

        // Act: Call the get scenes list endpoint.
        var listResponse = await _client.GetAsync("/scenes/get-scenes");

        // Assert: Validate that the response returns at least one scene.
        var internalErrorList = await ExtractInternalErrorAsync(listResponse);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await listResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorList}");

        var listApiResponse = await listResponse.Content.ReadFromJsonAsync<ApiResponse<List<SceneGetResponseDTO>>>();
        listApiResponse.Should().NotBeNull();
        listApiResponse!.Data.Should().NotBeNull();
        listApiResponse.Data.Should().NotBeEmpty();
    }


    [Fact]
    public async Task UpdateScene_ShouldReturnUpdatedScene()
    {
        // Arrange: Register, log in and create a scene.
        const string username = "sceneuser_update";
        const string email = "sceneuser_update@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        // Create a scene.
        var createDto = new SceneCreateDTO { Name = "Original Scene" };
        var createResponse = await _client.PostAsJsonAsync("/scenes/create-scene", createDto);
        var internalErrorCreate = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Scene creation failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorCreate}");

        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<SceneCreateResponseDTO>>();
        createApiResponse.Should().NotBeNull();
        var sceneId = createApiResponse!.Data.Id;
        sceneId.Should().NotBeEmpty();

        // Arrange: Prepare the update DTO.
        var updateDto = new SceneUpdateDTO
        {
            SceneId = sceneId,
            NewName = "Updated Scene"
        };

        // Act: Call the update endpoint.
        var updateResponse = await _client.PutAsJsonAsync("/scenes/update-scene", updateDto);

        // Assert: Validate the response contains the updated scene details.
        var internalErrorUpdate = await ExtractInternalErrorAsync(updateResponse);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await updateResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorUpdate}");

        var updateApiResponse = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<SceneUpdateResponseDTO>>();
        updateApiResponse.Should().NotBeNull();
        updateApiResponse!.Data.UpdatedName.Should().Be("Updated Scene");
        updateApiResponse.Data.SceneId.Should().Be(sceneId);
    }

    [Fact]
    public async Task RemoveScene_ShouldReturnOkAndDeleteScene()
    {
        // Arrange: Register, log in and create a scene.
        const string username = "sceneuser_remove";
        const string email = "sceneuser_remove@example.com";
        const string password = "TestPassword123!";
        var token = await RegisterAndLoginAsync(username, email, password);
        SetAuthorizationHeader(token);

        // Create a scene.
        var createDto = new SceneCreateDTO { Name = "Scene to Remove" };
        var createResponse = await _client.PostAsJsonAsync("/scenes/create-scene", createDto);
        var internalErrorCreate = await ExtractInternalErrorAsync(createResponse);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Scene creation failed. Raw response: {await createResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorCreate}");

        var createApiResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<SceneCreateResponseDTO>>();
        createApiResponse.Should().NotBeNull();
        var sceneId = createApiResponse!.Data.Id;
        sceneId.Should().NotBeEmpty();

        // Arrange: Prepare the remove DTO.
        var removeDto = new SceneRemoveDTO { SceneId = sceneId };

        // Act: Call the remove endpoint with an HTTP DELETE request that includes a JSON body.
        var request = new HttpRequestMessage(HttpMethod.Delete, "/scenes/remove-scene")
        {
            Content = JsonContent.Create(removeDto)
        };
        var removeResponse = await _client.SendAsync(request);

        // Assert: Validate that removal was successful.
        var internalErrorRemove = await ExtractInternalErrorAsync(removeResponse);
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Request failed. Raw response: {await removeResponse.Content.ReadAsStringAsync()} | Internal Error: {internalErrorRemove}");

        var removeApiResponse = await removeResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        removeApiResponse.Should().NotBeNull();
        removeApiResponse!.Data.Should().BeTrue();
    }
}