namespace NewsAggregator.Core.Entities;

/// <summary>Предпочтения пользователя по категориям</summary>
public class UserPreference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CategoryId { get; set; }
    
    /// <summary>Вес категории (чем больше читает — тем выше)</summary>
    public double Weight { get; set; } = 1.0;
}