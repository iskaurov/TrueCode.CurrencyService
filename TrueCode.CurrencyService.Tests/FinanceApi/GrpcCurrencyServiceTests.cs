using System.Text;
using System.Text.Json;
using Grpc.Net.Client;
using Grpc.Core;
using TrueCode.CurrencyService.FinanceApi.Grpc;

namespace TrueCode.CurrencyService.Tests.FinanceApi;

public class GrpcCurrencyServiceTests : IAsyncLifetime
{
    private GrpcChannel _channel = null!;
    private TrueCode.CurrencyService.FinanceApi.Grpc.CurrencyService.CurrencyServiceClient _client = null!;

    public async Task InitializeAsync()
    {
        _channel = GrpcChannel.ForAddress("http://localhost:5075");
        _client = new CurrencyService.FinanceApi.Grpc.CurrencyService.CurrencyServiceClient(_channel);
        await Task.CompletedTask; // здесь можно добавить подготовку окружения, если нужно
    }

    public async Task DisposeAsync()
    {
        await _channel.ShutdownAsync();
    }

    [Fact]
    public async Task GetFavorites_ShouldReturnList_WhenAuthorized()
    {
        // arrange
        var token = await GetJwtTokenAsync("iskaurov", "12345");

        var headers = new Metadata
        {
            { "Authorization", $"Bearer {token}" }
        };

        // act
        var response = await _client.GetFavoritesAsync(new FavoritesRequest(), headers);

        // assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Currencies);
    }

    [Fact]
    public async Task GetFavorites_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var ex = await Assert.ThrowsAsync<RpcException>(async () =>
        {
            await _client.GetFavoritesAsync(new FavoritesRequest());
        });

        Assert.Equal(StatusCode.Unauthenticated, ex.StatusCode);
    }

    private async Task<string> GetJwtTokenAsync(string username, string password)
    {
        using var client = new HttpClient();
        var content = new StringContent($$"""
            {
              "name": "{{username}}",
              "password": "{{password}}"
            }
            """, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5151/auth/login", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tokenObj = JsonSerializer.Deserialize<JsonElement>(json);

        return tokenObj.GetProperty("token").GetString()!;
    }
}