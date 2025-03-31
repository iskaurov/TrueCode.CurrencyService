using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TrueCode.CurrencyService.Infrastructure.Common;
using Program = TrueCode.CurrencyService.UserApi.Program;

namespace TrueCode.CurrencyService.Tests.Integration.UserApi;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        EnvLoader.Load("../../../../");
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldSucceed_WithValidData()
    {
        // Arrange
        var payload = new
        {
            name = $"testuser_{Guid.NewGuid():N}",
            password = "StrongPass123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Login_ShouldReturnToken_WithValidCredentials()
    {
        // Arrange
        var name = $"testuser_{Guid.NewGuid():N}";
        var password = "Test123!";

        // Сначала регистрируем пользователя
        var registerPayload = new { name, password };
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerPayload);
        registerResponse.EnsureSuccessStatusCode();

        // Затем логинимся
        var loginPayload = new { name, password };
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginPayload);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
    }
    
    [Fact]
    public async Task Login_ShouldFail_WithWrongPassword()
    {
        // Arrange
        var name = $"testuser_{Guid.NewGuid():N}";
        var password = "Test123!";
        var wrongPassword = "WrongPass456";

        // Регистрация
        var registerPayload = new { name, password };
        await _client.PostAsJsonAsync("/auth/register", registerPayload);

        // Попытка входа с неправильным паролем
        var loginPayload = new { name, password = wrongPassword };
        var response = await _client.PostAsJsonAsync("/auth/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Login_ShouldFail_WithNonexistentUser()
    {
        var payload = new
        {
            name = $"nonexistent_{Guid.NewGuid():N}",
            password = "AnyPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Login_ShouldFail_WithEmptyFields()
    {
        var payload = new { name = "", password = "" };

        var response = await _client.PostAsJsonAsync("/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    private class TokenResponse
    {
        public string Token { get; set; } = default!;
    }
}