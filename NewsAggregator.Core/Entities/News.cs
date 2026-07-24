namespace NewsAggregator.Core.Entities;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FullContent { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    
    /// <summary>Дата публикации с часовым поясом</summary>
    public DateTimeOffset PublishedDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    /// <summary>Количество просмотров (для рекомендаций)</summary>
    public int ViewCount { get; set; } = 0;
}