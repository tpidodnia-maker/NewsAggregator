using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly IParserService _parserService;

    public NewsController(INewsService newsService, IParserService parserService)
    {
        _newsService   = newsService;
        _parserService = parserService;
    }
    [HttpGet]
    public async Task<IActionResult> GetNews([FromQuery] NewsQueryParams queryParams)
    {
        var result = await _newsService.GetNewsAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetNewsById(int id)
    {
        var news = await _newsService.GetNewsByIdAsync(id);
        if (news == null) return NotFound(new { message = "Новость не найдена" });
        return Ok(news);
    }

    [HttpPost("parse")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ParseNews()
    {
        var news  = await _parserService.ParseAllSourcesAsync();
        var saved = await _newsService.SaveNewsAsync(news);
        return Ok(new { message = "Парсинг завершён", totalParsed = news.Count, savedToDb = saved });
    }
}