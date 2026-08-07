using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.Infrastructure.BackgroundServices;

public class NewsParserBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NewsParserBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

    public NewsParserBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NewsParserBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновый парсер запущен");
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope    = _scopeFactory.CreateScope();
                var parser         = scope.ServiceProvider.GetRequiredService<IParserService>();
                var newsService    = scope.ServiceProvider.GetRequiredService<INewsService>();
                var news           = await parser.ParseAllSourcesAsync();
                var saved          = await newsService.SaveNewsAsync(news);
                _logger.LogInformation("Автопарсинг: сохранено {Count}", saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом парсере");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}