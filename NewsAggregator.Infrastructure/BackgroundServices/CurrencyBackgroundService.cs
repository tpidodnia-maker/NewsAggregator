using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.Infrastructure.BackgroundServices;

/// <summary>Обновляет курсы валют каждый час</summary>
public class CurrencyBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CurrencyBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public CurrencyBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CurrencyBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис обновления валют запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope      = _scopeFactory.CreateScope();
                var currencyService  = scope.ServiceProvider
                    .GetRequiredService<ICurrencyService>();
                var rates = await currencyService.GetRatesAsync();
                _logger.LogInformation(
                    "Курсы валют обновлены: {Count} валют", rates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления курсов валют");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}