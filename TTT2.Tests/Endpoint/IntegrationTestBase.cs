using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.Authentication;
using TTT2.Tests.Factories;


namespace TTT2.Tests.Endpoint;

[Trait("Category", "Endpoint")]
public abstract class IntegrationTestBase(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient _client = factory.CreateClient();

    protected async Task<string> RegisterAndLoginAsync(string username, string email, string password)
    {
        // Register a new user.
        var registrationDto = new
        {
            username,
            email,
            password
        };
        var registerResponse = await _client.PostAsJsonAsync("/authentication/register", registrationDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Log in with the registered user.
        var loginDto = new
        {
            username, password
        };
        var loginResponse = await _client.PostAsJsonAsync("/authentication/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginApiResponse = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDTO>>();
        loginApiResponse.Should().NotBeNull();
        loginApiResponse!.Data.Should().NotBeNull();

        return loginApiResponse.Data.Token;
    }
        
    protected void SetAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
        
    protected static async Task<string?> ExtractInternalErrorAsync(HttpResponseMessage response)
    {
        var rawJson = await response.Content.ReadAsStringAsync();
        string? internalError = null;
        try
        {
            using var document = JsonDocument.Parse(rawJson);
            if (document.RootElement.TryGetProperty("internalMessage", out var internalMessageProp))
            {
                internalError = internalMessageProp.GetString();
            }
        }
        catch
        {
            internalError = "Unable to parse raw JSON response.";
        }
        return internalError;
    }
}