namespace NewsAggregator.Core.Entities;

/// <summary>История прочитанных новостей — основа для рекомендаций</summary>
public class UserReadHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int NewsId { get; set; }
    public News News { get; set; } = null!;
    public int CategoryId { get; set; }
    public DateTimeOffset ReadAt { get; set; } = DateTimeOffset.UtcNow;
}