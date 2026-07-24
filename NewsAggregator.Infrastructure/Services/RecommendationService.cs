using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Interfaces;
using NewsAggregator.Infrastructure.Data;

namespace NewsAggregator.Infrastructure.Services;

/// <summary>
/// Рекомендации на основе истории просмотров пользователя.
/// Алгоритм: берём топ-3 категории пользователя → выдаём свежие новости из них.
/// </summary>
public class RecommendationService : IRecommendationService
{
    private readonly AppDbContext _db;

    public RecommendationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<NewsDto>> GetRecommendationsAsync(int userId, int count = 10)
    {
        // Берём топ категории которые пользователь читал чаще всего
        var topCategories = await _db.UserReadHistories
            .Where(h => h.UserId == userId)
            .GroupBy(h => h.CategoryId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync();

        // Если истории нет — возвращаем просто свежие новости
        if (!topCategories.Any())
        {
            return await _db.News
                .Include(n => n.Category)
                .OrderByDescending(n => n.PublishedDate)
                .Take(count)
                .Select(n => MapToDto(n))
                .ToListAsync();
        }

        // Новости из топ категорий, которые пользователь ещё не читал
        var readNewsIds = await _db.UserReadHistories
            .Where(h => h.UserId == userId)
            .Select(h => h.NewsId)
            .ToListAsync();

        var recommended = await _db.News
            .Include(n => n.Category)
            .Where(n => topCategories.Contains(n.CategoryId) && !readNewsIds.Contains(n.Id))
            .OrderByDescending(n => n.PublishedDate)
            .Take(count)
            .Select(n => MapToDto(n))
            .ToListAsync();

        return recommended;
    }

    public async Task TrackReadAsync(int userId, int newsId, int categoryId)
    {
        // Не дублируем если уже читал
        var exists = await _db.UserReadHistories
            .AnyAsync(h => h.UserId == userId && h.NewsId == newsId);

        if (!exists)
        {
            _db.UserReadHistories.Add(new Core.Entities.UserReadHistory
            {
                UserId     = userId,
                NewsId     = newsId,
                CategoryId = categoryId,
                ReadAt     = DateTimeOffset.UtcNow
            });

            // Увеличиваем ViewCount
            var news = await _db.News.FindAsync(newsId);
            if (news != null) news.ViewCount++;

            await _db.SaveChangesAsync();
        }
    }

    private static NewsDto MapToDto(Core.Entities.News n) => new()
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
        ViewCount    = n.ViewCount
    };
}