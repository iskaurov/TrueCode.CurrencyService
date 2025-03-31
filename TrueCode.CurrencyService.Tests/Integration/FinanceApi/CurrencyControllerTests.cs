using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TrueCode.CurrencyService.Core.Models;
using Program = TrueCode.CurrencyService.FinanceApi.Program;

namespace TrueCode.CurrencyService.Tests.Integration.FinanceApi;

public class CurrencyControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetFavorites_ShouldReturn401_IfTokenMissing()
    {
        var response = await _client.GetAsync("/currency/favorites");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public async Task<string> GetToken(string name, string password)
    {
        var userApiFactory = new WebApplicationFactory<TrueCode.CurrencyService.UserApi.Program>();
        var userClient = userApiFactory.CreateClient();
        var loginPayload = new { name, password };
        var loginResponse = await userClient.PostAsJsonAsync("/auth/login", loginPayload);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        var token = loginResult!.Token;
        return token;
    }
    
    [Fact]
    public async Task GetFavorites_ShouldReturn200_WithValidToken()
    {
        // Arrange: получаем токен из UserApi
        var userApiFactory = new WebApplicationFactory<TrueCode.CurrencyService.UserApi.Program>();
        var userClient = userApiFactory.CreateClient();

        var name = $"testuser_{Guid.NewGuid():N}";
        var password = "Test123!";

        var registerPayload = new { name, password };
        await userClient.PostAsJsonAsync("/auth/register", registerPayload);
        
        var token = await Helper.GetToken(name, password);

        // Act: обращаемся к /currency/favorites с токеном
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/currency/favorites");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFavorites_ShouldReturn401_IfNoTokenProvided()
    {
        // Act
        var response = await _client.GetAsync("/currency/favorites");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetFavorites_ShouldReturn401_IfTokenIsInvalid()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "this.is.not.valid");

        // Act
        var response = await _client.GetAsync("/currency/favorites");
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}