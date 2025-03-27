using TrueCode.CurrencyService.Domain.Entities;

namespace TrueCode.CurrencyService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
    Task AddAsync(User user);
    
    Task<User?> GetUserWithFavoritesAsync(Guid userId);
}