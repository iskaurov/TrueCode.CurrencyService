using TrueCode.CurrencyService.FinanceApi.Models;

namespace TrueCode.CurrencyService.Application.Services;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDto>> GetFavoritesByUserIdAsync(Guid userId);
}