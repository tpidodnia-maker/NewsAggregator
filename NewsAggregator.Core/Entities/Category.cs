namespace NewsAggregator.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "📰"; // Иконка для UI
    
    public ICollection<News> News { get; set; } = new List<News>();
}