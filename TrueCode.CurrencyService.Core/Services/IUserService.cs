namespace TrueCode.CurrencyService.Core.Services;

public interface IUserService
{
    Task RegisterAsync(string name, string password);
    Task<string> LoginAsync(string name, string password);
}