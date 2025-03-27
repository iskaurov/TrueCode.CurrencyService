using Microsoft.EntityFrameworkCore;
using TrueCode.CurrencyService.CurrencyUpdater;
using TrueCode.CurrencyService.Infrastructure.Db;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();

        services.AddDbContext<CurrencyDbContext>(options =>
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

        services.Configure<CurrencyUpdaterOptions>(
            context.Configuration.GetSection("CurrencyUpdater"));
        
        services.AddHostedService<CurrencyUpdateWorker>();
    })
    .Build()
    .Run();