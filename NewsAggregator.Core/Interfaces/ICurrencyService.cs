using NewsAggregator.Core.DTOs;

namespace NewsAggregator.Core.Interfaces;

public interface ICurrencyService
{
    Task<List<CurrencyRateDto>> GetRatesAsync();
}