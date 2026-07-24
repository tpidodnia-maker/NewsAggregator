using NewsAggregator.Core.DTOs;

namespace NewsAggregator.Core.Interfaces;

public interface IRecommendationService
{
    Task<List<NewsDto>> GetRecommendationsAsync(int userId, int count = 10);
    Task TrackReadAsync(int userId, int newsId, int categoryId);
}