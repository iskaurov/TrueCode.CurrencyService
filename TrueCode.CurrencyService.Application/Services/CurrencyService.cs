using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.FinanceApi.Models;

namespace TrueCode.CurrencyService.Application.Services;

public class CurrencyService(IUserRepository userRepository) : ICurrencyService
{
    public async Task<IEnumerable<CurrencyDto>> GetFavoritesByUserIdAsync(Guid userId)
    {
        var user = await userRepository.GetUserWithFavoritesAsync(userId);

        if (user?.FavoriteCurrencies is null)
            return new List<CurrencyDto>();

        return user.FavoriteCurrencies
            .Select(c => new CurrencyDto { Name = c.Name, Rate = c.Rate })
            .ToList();
    }
}