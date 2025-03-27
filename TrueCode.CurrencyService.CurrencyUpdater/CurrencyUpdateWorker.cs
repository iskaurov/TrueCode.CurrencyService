using Microsoft.Extensions.Options;

using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
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

    private const string CurrenciesSourceUrl = "http://www.cbr.ru/scripts/XML_daily.asp";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Start updating currencies...");

                var httpClient = httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync(CurrenciesSourceUrl, stoppingToken);

                var doc = XDocument.Parse(response);
                var fetchedCurrencies = doc.Descendants("Valute")
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

                var existingCurrencies = await db.Currencies
                    .ToDictionaryAsync(c => c.Name, stoppingToken);

                foreach (var fetched in fetchedCurrencies)
                {
                    if (existingCurrencies.TryGetValue(fetched.Name, out var existing))
                        existing.Rate = fetched.Rate;
                    else
                        await db.Currencies.AddAsync(fetched, stoppingToken);
                }

                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Currencies updated: {Count}", fetchedCurrencies.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update currencies");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.UpdateIntervalMinutes), stoppingToken);
        }
    }
}