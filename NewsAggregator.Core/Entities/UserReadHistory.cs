namespace NewsAggregator.Core.Entities;

public class UserReadHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int NewsId { get; set; }
    public News News { get; set; } = null!;
    public int CategoryId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}