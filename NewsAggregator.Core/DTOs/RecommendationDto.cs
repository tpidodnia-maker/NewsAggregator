namespace NewsAggregator.Core.DTOs;

public class RecommendationDto
{
    public int UserId { get; set; }
    public int NewsId { get; set; }
    public int CategoryId { get; set; }
    public double Score { get; set; }
}