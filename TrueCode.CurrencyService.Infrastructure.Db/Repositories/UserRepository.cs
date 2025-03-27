using Microsoft.EntityFrameworkCore;
using TrueCode.CurrencyService.Domain.Entities;
using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.Infrastructure.Db;

namespace TrueCode.CurrencyService.Infrastructure.Repositories;

public class UserRepository(CurrencyDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByNameAsync(string name)
    {
        return await dbContext.Users
            .Include(u => u.FavoriteCurrencies)
            .FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await dbContext.Users.AnyAsync(u => u.Name == name);
    }

    public async Task AddAsync(User user)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task<User?> GetUserWithFavoritesAsync(Guid userId)
    {
        return await dbContext.Users
            .Include(u => u.FavoriteCurrencies)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}