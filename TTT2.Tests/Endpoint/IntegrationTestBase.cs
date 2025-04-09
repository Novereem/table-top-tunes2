using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Models.Common;
using Shared.Models.DTOs.AudioFiles;
using Shared.Models.DTOs.Authentication;
using TTT2.Tests.Factories;


namespace TTT2.Tests.Endpoint
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly HttpClient _client;
        protected readonly CustomWebApplicationFactory _factory;

        public IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        
        protected async Task<string> RegisterAndLoginAsync(string username, string email, string password)
        {
            // Register a new user.
            var registrationDto = new
            {
                username = username,
                email = email,
                password = password
            };
            var registerResponse = await _client.PostAsJsonAsync("/authentication/register", registrationDto);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Log in with the registered user.
            var loginDto = new
            {
                username = username,
                password = password
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
    }
}