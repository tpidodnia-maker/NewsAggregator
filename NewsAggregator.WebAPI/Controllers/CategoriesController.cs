using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Infrastructure.Data;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _db.Categories
            .Select(c => new CategoryDto
            {
                Id          = c.Id,
                Name        = c.Name,
                Icon        = c.Icon,
                Description = c.Description,
                NewsCount   = c.News.Count
            })
            .ToListAsync();

        return Ok(categories);
    }
}