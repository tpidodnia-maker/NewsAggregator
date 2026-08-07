namespace NewsAggregator.Core.Entities;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FullContent { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int ViewCount { get; set; } = 0;

    /// <summary>Ссылка на изображение новости. Может отсутствовать (null),
    /// если источник не отдал картинку — тогда фронтенд показывает плейсхолдер.</summary>
    public string? ImageUrl { get; set; }
}