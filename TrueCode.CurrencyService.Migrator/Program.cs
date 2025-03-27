using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using TrueCode.CurrencyService.Infrastructure.Db;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
   
var services = new ServiceCollection();

services.AddDbContext<CurrencyDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();

Console.WriteLine("Applying migrations...");
db.Database.Migrate();
Console.WriteLine("Done.");