// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Configuration.Json;
// using Microsoft.Extensions.DependencyInjection;
// using TrueCode.CurrencyService.Infrastructure.Db;
//
// var configuration = new ConfigurationBuilder()
//     .AddJsonFile("appsettings.json")
//     .Build();
//    
// var services = new ServiceCollection();
//
// services.AddDbContext<CurrencyDbContext>(options =>
//     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
//
// var serviceProvider = services.BuildServiceProvider();
//
// using var scope = serviceProvider.CreateScope();
//
// var db = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();
//
// Console.WriteLine("Applying migrations...");
// db.Database.Migrate();
// Console.WriteLine("Done.");

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrueCode.CurrencyService.Infrastructure.Db;

// === Подгружаем переменные окружения из ../.env ===
var envPath = Path.Combine(AppContext.BaseDirectory, "../.env");
if (File.Exists(envPath))
{
    var lines = File.ReadAllLines(envPath);
    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

// === Проверяем нужно ли применять миграции ===
var applyMigrations = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");
if (applyMigrations?.ToLowerInvariant() != "true")
{
    Console.WriteLine("⚠️ Миграции отключены (APPLY_MIGRATIONS != true)");
    return;
}

// === Собираем конфигурацию из окружения ===
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

// === Настраиваем DI и БД ===
var services = new ServiceCollection();

var connectionString = configuration["DEFAULT_CONNECTION"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("❌ Строка подключения DEFAULT_CONNECTION не найдена");
    return;
}

services.AddDbContext<CurrencyDbContext>(options =>
    options.UseNpgsql(connectionString));

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();

// === Применяем миграции ===
Console.WriteLine("🛠 Применяем миграции...");
await db.Database.MigrateAsync();
Console.WriteLine("✅ Миграции успешно применены.");