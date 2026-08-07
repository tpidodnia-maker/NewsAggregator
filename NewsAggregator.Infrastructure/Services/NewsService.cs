using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Entities;
using NewsAggregator.Core.Interfaces;
using NewsAggregator.Infrastructure.Data;

namespace NewsAggregator.Infrastructure.Services;

public class NewsService : INewsService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NewsService> _logger;

    public NewsService(AppDbContext db, IMemoryCache cache, ILogger<NewsService> logger)
    {
        _db    = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResult<NewsDto>> GetNewsAsync(NewsQueryParams q)
    {
        var query = _db.News.Include(n => n.Category).AsQueryable();

        if (q.CategoryId.HasValue)
            query = query.Where(n => n.CategoryId == q.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(n =>
                n.Title.Contains(q.Search) || n.Content.Contains(q.Search));

        if (q.DateFrom.HasValue)
            query = query.Where(n => n.PublishedDate >= q.DateFrom.Value);

        if (q.DateTo.HasValue)
            query = query.Where(n => n.PublishedDate <= q.DateTo.Value);

        query = q.SortBy switch
        {
            "title" => query.OrderBy(n => n.Title),
            "views" => query.OrderByDescending(n => n.ViewCount),
            _       => query.OrderByDescending(n => n.PublishedDate)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(n => new NewsDto
            {
                Id           = n.Id,
                Title        = n.Title,
                Content      = n.Content,
                Url          = n.Url,
                Source       = n.Source,
                PublishedDate = n.PublishedDate,
                CategoryName = n.Category.Name,
                CategoryIcon = n.Category.Icon,
                CategoryId   = n.CategoryId,
                ViewCount    = n.ViewCount,
                ImageUrl     = n.ImageUrl
            })
            .ToListAsync();

        return new PagedResult<NewsDto>
        {
            Items      = items,
            TotalCount = total,
            Page       = q.Page,
            PageSize   = q.PageSize
        };
    }

    public async Task<NewsDetailDto?> GetNewsByIdAsync(int id)
    {
        var n = await _db.News
            .Include(n => n.Category)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (n == null) return null;

        return new NewsDetailDto
        {
            Id           = n.Id,
            Title        = n.Title,
            Content      = n.Content,
            FullContent  = n.FullContent,
            Url          = n.Url,
            Source       = n.Source,
            PublishedDate = n.PublishedDate,
            CategoryName = n.Category.Name,
            CategoryIcon = n.Category.Icon,
            CategoryId   = n.CategoryId,
            ViewCount    = n.ViewCount,
            CreatedAt    = n.CreatedAt,
            ImageUrl     = n.ImageUrl
        };
    }

    public async Task<int> SaveNewsAsync(IEnumerable<News> newsList)
    {
        int saved = 0, updated = 0;
        foreach (var news in newsList)
        {
            var existing = await _db.News.FirstOrDefaultAsync(n => n.Url == news.Url);
            if (existing != null)
            {
                // Дополняем уже существующую запись, если раньше не удалось найти картинку
                if (string.IsNullOrWhiteSpace(existing.ImageUrl) && !string.IsNullOrWhiteSpace(news.ImageUrl))
                {
                    existing.ImageUrl = news.ImageUrl;
                    updated++;
                }
                continue;
            }
            _db.News.Add(news);
            saved++;
        }
        await _db.SaveChangesAsync();
        _logger.LogInformation("Сохранено {Saved} новых, дополнено {Updated} существующих", saved, updated);
        return saved;
    }
}