namespace NewsAggregator.Core.DTOs;
public class NewsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset PublishedDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int ViewCount { get; set; }
}

public class NewsDetailDto : NewsDto
{
    public string? FullContent { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsRecommended { get; set; }
}

public class NewsQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CategoryId { get; set; }
    public string SortBy { get; set; } = "date";
    
    /// <summary>Поиск по периоду — начало</summary>
    public DateTimeOffset? DateFrom { get; set; }
    
    /// <summary>Поиск по периоду — конец</summary>
    public DateTimeOffset? DateTo { get; set; }
    
    /// <summary>Текстовый поиск</summary>
    public string? Search { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}