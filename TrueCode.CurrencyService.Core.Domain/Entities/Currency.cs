namespace TrueCode.CurrencyService.Domain.Entities;

public class Currency
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
}