namespace TrueCode.CurrencyService.WebApi.UserApi.Models.Auth;

public class RegisterRequest
{
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;
}