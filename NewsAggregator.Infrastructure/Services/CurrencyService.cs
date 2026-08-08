using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Interfaces;
using System.Text.Json;

namespace NewsAggregator.Infrastructure.Services;

/// <summary>
/// Получает курсы валют через бесплатный API exchangerate-api.com
/// Кэширует на 1 час
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyService> _logger;
    private const string CacheKey = "currency_rates";

    // Валюты которые показываем (курс — сколько рублей за 1 ед.)
    private static readonly Dictionary<string, (string Name, string Flag)> Currencies = new()
    {
        ["USD"] = ("Доллар США",        "🇺🇸"),
        ["EUR"] = ("Евро",              "🇪🇺"),
        ["GBP"] = ("Фунт стерлингов",  "🇬🇧"),
        ["JPY"] = ("Японская иена",     "🇯🇵"),
        ["CNY"] = ("Китайский юань",    "🇨🇳"),
        ["CHF"] = ("Швейцарский франк", "🇨🇭"),
        ["CAD"] = ("Канадский доллар",  "🇨🇦"),
        ["AUD"] = ("Австралийский $",   "🇦🇺"),
    };

    public CurrencyService(HttpClient http, IMemoryCache cache, ILogger<CurrencyService> logger)
    {
        _http   = http;
        _cache  = cache;
        _logger = logger;
    }

    public async Task<List<CurrencyRateDto>> GetRatesAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<CurrencyRateDto>? cached) && cached != null)
            return cached;

        try
        {
            // Базовая валюта — российский рубль: курсы отдают «сколько ₽ за 1 ед. валюты»
            var response = await _http.GetStringAsync(
                "https://api.exchangerate-api.com/v4/latest/RUB");

            var json  = JsonDocument.Parse(response);
            var rates = json.RootElement.GetProperty("rates");

            var result = new List<CurrencyRateDto>();
            foreach (var (code, info) in Currencies)
            {
                if (rates.TryGetProperty(code, out var rateEl))
                {
                    var forOne = rateEl.GetDecimal();
                    if (forOne <= 0) continue;

                    result.Add(new CurrencyRateDto
                    {
                        Code      = code,
                        Name      = info.Name,
                        Flag      = info.Flag,
                        // API с базой RUB даёт «сколько валюты за 1₽» — инвертируем в «₽ за 1 ед.»
                        Rate      = 1m / forOne,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            _cache.Set(CacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения курсов валют");
            return new List<CurrencyRateDto>();
        }
    }
}