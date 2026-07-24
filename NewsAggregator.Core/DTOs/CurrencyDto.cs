namespace NewsAggregator.Core.DTOs;

public class CurrencyRateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal Change { get; set; }
    public string Flag { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}