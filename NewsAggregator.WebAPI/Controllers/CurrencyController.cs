using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currency;

    public CurrencyController(ICurrencyService currency) => _currency = currency;

    [HttpGet]
    public async Task<IActionResult> GetRates()
    {
        var rates = await _currency.GetRatesAsync();
        return Ok(rates);
    }
}