namespace TrueCode.CurrencyService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Password { get; set; } = null!;
    
    public ICollection<Currency> FavoriteCurrencies { get; set; } = new List<Currency>();
}