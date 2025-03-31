using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TrueCode.CurrencyService.Core.Models;

namespace TrueCode.CurrencyService.Tests;

public static class Helper
{
    public static async Task<string> GetToken(string name, string password)
    {
        var userApiFactory = new WebApplicationFactory<WebApi.UserApi.Program>();
        var userClient = userApiFactory.CreateClient();
        var loginPayload = new { name, password };
        var loginResponse = await userClient.PostAsJsonAsync("/auth/login", loginPayload);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return loginResult!.Token;
    }
}