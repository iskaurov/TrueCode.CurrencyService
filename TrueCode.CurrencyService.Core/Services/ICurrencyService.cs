using TrueCode.CurrencyService.FinanceApi.Models;

namespace TrueCode.CurrencyService.Core.Services;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDto>> GetFavoritesByUserIdAsync(Guid userId);
}