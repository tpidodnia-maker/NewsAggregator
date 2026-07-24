using Microsoft.Extensions.Logging;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.Infrastructure.Services;

public class ClassifierService : IClassifierService
{
    private readonly ILogger<ClassifierService> _logger;

    private static readonly Dictionary<string, string[]> Keywords = new()
    {
        ["Политика"] = new[]
        {
            "president", "election", "parliament", "government", "senator",
            "congress", "minister", "policy", "diplomat", "sanction", "treaty",
            "президент", "выборы", "парламент", "правительство", "депутат",
            "закон", "министр", "санкции"
        },
        ["Экономика"] = new[]
        {
            "economy", "inflation", "bitcoin", "stock", "market", "crisis",
            "currency", "budget", "bank", "gdp", "trade", "investment", "crypto",
            "экономика", "инфляция", "биткоин", "акции", "кризис", "валюта",
            "бюджет", "банк"
        },
        ["Спорт"] = new[]
        {
            "football", "soccer", "championship", "match", "goal", "olympic",
            "coach", "athlete", "tournament", "nba", "nfl", "fifa", "tennis",
            "спорт", "футбол", "чемпионат", "матч", "гол", "олимпиада", "тренер"
        },
        ["Технологии"] = new[]
        {
            "artificial intelligence", "ai", "neural", "computer", "programming",
            "robot", "software", "startup", "cybersecurity", "hack", "apple",
            "google", "microsoft", "openai", "технологии", "нейросеть",
            "программирование", "робот", "кибербезопасность"
        },
        ["Наука"] = new[]
        {
            "research", "discovery", "space", "gene", "quantum", "experiment",
            "nasa", "climate", "vaccine", "scientist", "biology", "physics",
            "наука", "исследование", "открытие", "космос", "климат", "вакцина"
        },
        ["Культура"] = new[]
        {
            "film", "movie", "exhibition", "concert", "museum", "book",
            "theater", "art", "music", "festival", "oscar", "grammy",
            "культура", "кино", "выставка", "концерт", "музей", "театр"
        },
        ["Здоровье"] = new[]
        {
            "health", "medicine", "hospital", "disease", "treatment", "doctor",
            "pandemic", "virus", "здоровье", "медицина", "больница", "болезнь"
        },
        ["Бизнес"] = new[]
        {
            "business", "company", "startup", "ceo", "merger", "acquisition",
            "revenue", "profit", "бизнес", "компания", "прибыль", "сделка"
        },
        ["Экология"] = new[]
        {
            "climate", "environment", "green", "carbon", "emission", "renewable",
            "экология", "климат", "окружающая среда", "выбросы", "зелёная энергия"
        },
        ["Развлечения"] = new[]
        {
            "entertainment", "game", "movie", "series", "streaming", "netflix",
            "развлечения", "игра", "сериал", "кино", "стриминг"
        }
    };

    public ClassifierService(ILogger<ClassifierService> logger)
    {
        _logger = logger;
    }

    public string Classify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Другое";
        var lowerText = text.ToLowerInvariant();
        int maxScore  = 0;
        string best   = "Другое";

        foreach (var category in Keywords)
        {
            int score = category.Value.Count(kw =>
                lowerText.Contains(kw, StringComparison.OrdinalIgnoreCase));

            if (score > maxScore)
            {
                maxScore = score;
                best     = category.Key;
            }
        }

        return best;
    }
}