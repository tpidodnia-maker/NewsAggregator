using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.Interfaces;
using System.Security.Claims;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _rec;

    public RecommendationsController(IRecommendationService rec) => _rec = rec;

    [HttpGet]
    public async Task<IActionResult> GetRecommendations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _rec.GetRecommendationsAsync(userId);
        return Ok(result);
    }
    [HttpPost("track/{newsId:int}")]
    public async Task<IActionResult> TrackRead(int newsId, [FromQuery] int categoryId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _rec.TrackReadAsync(userId, newsId, categoryId);
        return Ok();
    }
}