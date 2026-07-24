namespace NewsAggregator.Core.Entities;

public class User
{
    public int Id { get; set; }
    
    /// <summary>Уникальный username</summary>
    public string Username { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>Роль: User / Admin (только один Admin)</summary>
    public string Role { get; set; } = "User";
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiry { get; set; }
    
    // Навигационные свойства
    public ICollection<UserReadHistory> ReadHistory { get; set; } = new List<UserReadHistory>();
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
}