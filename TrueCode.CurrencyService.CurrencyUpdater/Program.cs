using Microsoft.EntityFrameworkCore;
using TrueCode.CurrencyService.CurrencyUpdater;
using TrueCode.CurrencyService.Infrastructure.Db;
using TrueCode.CurrencyService.Infrastructure.Common;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var envFile = envName == "Development" ? ".env.dev" : ".env";

// Загружаем переменные из .env (лежит уровнем выше)
EnvLoader.Load("../", envFile);

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();

        services.AddDbContext<CurrencyDbContext>(options =>
            options.UseNpgsql(context.Configuration["DEFAULT_CONNECTION"]));

        services.Configure<CurrencyUpdaterOptions>(
            context.Configuration.GetSection("CurrencyUpdater"));
        
        services.AddHostedService<CurrencyUpdateWorker>();
    })
    .Build()
    .Run();