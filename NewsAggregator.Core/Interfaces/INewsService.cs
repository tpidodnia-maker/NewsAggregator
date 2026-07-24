using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Entities;

namespace NewsAggregator.Core.Interfaces;

public interface INewsService
{
    Task<PagedResult<NewsDto>> GetNewsAsync(NewsQueryParams queryParams);
    Task<NewsDetailDto?> GetNewsByIdAsync(int id);
    Task<int> SaveNewsAsync(IEnumerable<News> newsList);
}