using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Models.Common;
using Shared.Models.DTOs.Authentication;
using TTT2.Tests.Factories;

namespace TTT2.Tests.Endpoint
{
    public class AuthenticationEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthenticationEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterUser_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var registrationDto = new RegisterDTO
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/authentication/register", registrationDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Deserialize the API response (which is created by your ToActionResult extension)
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponseDTO>>();
            apiResponse.Should().NotBeNull();
            apiResponse.Data.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task RegisterUser_DuplicateUser_ShouldReturnConflict()
        {
            // Arrange
            var registrationDto = new RegisterDTO
            {
                Username = "duplicateuser",
                Email = "dup@example.com",
                Password = "TestPassword123!"
            };

            // Act: First register, then try to register again
            await _client.PostAsJsonAsync("/authentication/register", registrationDto);
            var response = await _client.PostAsJsonAsync("/authentication/register", registrationDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task RegisterUser_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidDto = new RegisterDTO
            {
                Username = "", // Invalid username
                Email = "not-an-email", // Likely invalid
                Password = "password"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/authentication/register", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOk()
        {
            // Arrange: Register a user first
            var registrationDto = new RegisterDTO
            {
                Username = "loginuser",
                Email = "loginuser@example.com",
                Password = "TestPassword123!"
            };
            await _client.PostAsJsonAsync("/authentication/register", registrationDto);

            var loginDto = new LoginDTO
            {
                Username = "loginuser",
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/authentication/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDTO>>();
            apiResponse.Should().NotBeNull();
            apiResponse.Data.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange: Register the user first
            var registrationDto = new RegisterDTO
            {
                Username = "loginuser2",
                Email = "loginuser2@example.com",
                Password = "TestPassword123!"
            };
            await _client.PostAsJsonAsync("/authentication/register", registrationDto);

            var loginDto = new LoginDTO
            {
                Username = "loginuser2",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/authentication/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_NonExistentUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Username = "nonexistent",
                Password = "AnyPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/authentication/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}