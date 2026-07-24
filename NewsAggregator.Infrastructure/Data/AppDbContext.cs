using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core.Entities;

namespace NewsAggregator.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<News> News => Set<News>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserReadHistory> UserReadHistories => Set<UserReadHistory>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Уникальные индексы
        modelBuilder.Entity<News>().HasIndex(n => n.Url).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // Расширенные категории с иконками
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1,  Name = "Политика",         Icon = "🏛️",  Description = "Политические новости" },
            new Category { Id = 2,  Name = "Экономика",        Icon = "💹",  Description = "Экономика и финансы" },
            new Category { Id = 3,  Name = "Спорт",            Icon = "⚽",  Description = "Спортивные события" },
            new Category { Id = 4,  Name = "Технологии",       Icon = "💻",  Description = "IT и технологии" },
            new Category { Id = 5,  Name = "Наука",            Icon = "🔬",  Description = "Научные открытия" },
            new Category { Id = 6,  Name = "Культура",         Icon = "🎭",  Description = "Культура и искусство" },
            new Category { Id = 7,  Name = "Здоровье",         Icon = "🏥",  Description = "Медицина и здоровье" },
            new Category { Id = 8,  Name = "Бизнес",           Icon = "💼",  Description = "Бизнес и предпринимательство" },
            new Category { Id = 9,  Name = "Экология",         Icon = "🌿",  Description = "Окружающая среда" },
            new Category { Id = 10, Name = "Развлечения",      Icon = "🎬",  Description = "Кино, музыка, игры" },
            new Category { Id = 11, Name = "Образование",      Icon = "📚",  Description = "Наука и образование" },
            new Category { Id = 12, Name = "Путешествия",      Icon = "✈️",  Description = "Туризм и путешествия" },
            new Category { Id = 13, Name = "Другое",           Icon = "📰",  Description = "Разное" }
        );
    }
}