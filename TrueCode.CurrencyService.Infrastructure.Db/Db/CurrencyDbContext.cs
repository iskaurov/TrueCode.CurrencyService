using Microsoft.EntityFrameworkCore;
using TrueCode.CurrencyService.Domain.Entities;

namespace TrueCode.CurrencyService.Infrastructure.Db;

public class CurrencyDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Currency> Currencies => Set<Currency>();

    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteCurrencies)
            .WithMany()
            .UsingEntity(j=>j.ToTable("UserCurrency"));
        
        base.OnModelCreating(modelBuilder);
        // Можно добавить конфигурации вручную или через IEntityTypeConfiguration
    }
}