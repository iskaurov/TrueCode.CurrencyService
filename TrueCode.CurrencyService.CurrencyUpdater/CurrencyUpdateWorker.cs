using Microsoft.Extensions.Options;

using System.Globalization;
using System.Xml.Linq;
using TrueCode.CurrencyService.Infrastructure.Db;
using TrueCode.CurrencyService.Domain.Entities;

namespace TrueCode.CurrencyService.CurrencyUpdater;

public class CurrencyUpdateWorker(
    ILogger<CurrencyUpdateWorker> logger,
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider,
    IOptions<CurrencyUpdaterOptions> options)
    : BackgroundService
{
    private readonly CurrencyUpdaterOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Start updating currencies...");

                var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("http://www.cbr.ru/scripts/XML_daily.asp", stoppingToken);

                var doc = XDocument.Parse(response);
                var currencies = doc.Descendants("Valute")
                    .Select(valute => new Currency
                    {
                        Id = Guid.NewGuid(),
                        Name = valute.Element("CharCode")?.Value ?? "N/A",
                        Rate = decimal.Parse(
                            valute.Element("Value")?.Value.Replace(",", ".") ?? "0",
                            CultureInfo.InvariantCulture
                        )
                    })
                    .ToList();

                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();

                // Удалим все и вставим заново (можно улучшить на UPSERT)
                db.Currencies.RemoveRange(db.Currencies);
                await db.Currencies.AddRangeAsync(currencies, stoppingToken);
                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Currencies updated: {Count}", currencies.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update currencies");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(_options.UpdateIntervalMinutes), stoppingToken);
        }
    }
}