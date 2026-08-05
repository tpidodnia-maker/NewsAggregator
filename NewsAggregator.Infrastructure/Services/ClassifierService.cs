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
            "президент", "путин", "байден", "трамп", "выборы", "парламент",
            "правительство", "депутат", "закон", "министр", "санкции", "дума",
            "кремль", "сенат", "политик", "голосование", "партия", "оппозиция",
            "посол", "дипломат", "переговоры", "саммит", "нато", "оон",
            "president", "election", "government", "parliament", "senate",
            "congress", "minister", "policy", "diplomat", "sanction", "treaty",
            "мид", "госдеп", "байден", "зеленский", "макрон", "шольц"
        },
        ["Экономика"] = new[]
        {
            "экономика", "инфляция", "биткоин", "акции", "кризис", "валюта",
            "бюджет", "банк", "торговля", "инвестиции", "рубль", "доллар",
            "евро", "нефть", "газ", "ввп", "рост", "падение", "рынок",
            "цены", "налог", "пошлина", "экспорт", "импорт", "санкции",
            "economy", "inflation", "bitcoin", "stock", "market", "currency",
            "budget", "bank", "trade", "investment", "recession", "crypto",
            "цб", "центробанк", "минфин", "ставка", "кредит", "ипотека"
        },
        ["Спорт"] = new[]
        {
            "футбол", "чемпионат", "матч", "гол", "олимпиада", "тренер",
            "турнир", "теннис", "баскетбол", "хоккей", "спорт", "игрок",
            "команда", "победа", "поражение", "финал", "лига", "кубок",
            "football", "soccer", "championship", "match", "goal", "olympic",
            "coach", "athlete", "tournament", "nba", "nfl", "fifa", "tennis",
            "цска", "спартак", "зенит", "локомотив", "динамо", "рпл"
        },
        ["Технологии"] = new[]
        {
            "технологии", "искусственный интеллект", "нейросеть", "компьютер",
            "программирование", "робот", "кибербезопасность", "хакер", "взлом",
            "смартфон", "приложение", "интернет", "цифровой", "стартап",
            "artificial intelligence", "ai", "neural", "computer", "programming",
            "robot", "software", "startup", "cybersecurity", "hack",
            "apple", "google", "microsoft", "openai", "яндекс", "сбер"
        },
        ["Наука"] = new[]
        {
            "наука", "исследование", "открытие", "космос", "климат", "вакцина",
            "учёный", "эксперимент", "технология", "физика", "химия", "биология",
            "research", "discovery", "space", "gene", "quantum", "experiment",
            "nasa", "climate", "vaccine", "scientist", "biology", "physics",
            "роскосмос", "ракета", "спутник", "марс", "луна"
        },
        ["Культура"] = new[]
        {
            "кино", "выставка", "концерт", "музей", "театр", "искусство",
            "музыка", "фестиваль", "книга", "фильм", "актёр", "режиссёр",
            "премия", "оскар", "певец", "певица", "шоу", "сериал",
            "film", "movie", "exhibition", "concert", "museum", "book",
            "theater", "art", "music", "festival", "oscar", "grammy",
            "пугачева", "галкин", "юрмала", "кинофестиваль"
        },
        ["Здоровье"] = new[]
        {
            "здоровье", "медицина", "больница", "болезнь", "лечение", "врач",
            "пандемия", "вирус", "вакцина", "препарат", "операция", "диагноз",
            "health", "medicine", "hospital", "disease", "treatment", "doctor",
            "pandemic", "virus", "минздрав", "роспотребнадзор", "онкология"
        },
        ["Бизнес"] = new[]
        {
            "бизнес", "компания", "прибыль", "сделка", "слияние", "акционер",
            "генеральный директор", "выручка", "убыток", "банкротство",
            "business", "company", "startup", "ceo", "merger", "revenue",
            "profit", "корпорация", "холдинг", "ipo", "биржа"
        },
        ["Экология"] = new[]
        {
            "экология", "климат", "окружающая среда", "выбросы", "загрязнение",
            "зелёная энергия", "солнечная", "ветровая", "углерод",
            "climate", "environment", "green", "carbon", "emission", "renewable"
        },
        ["Развлечения"] = new[]
        {
            "развлечения", "игра", "стриминг", "netflix", "кино", "аниме",
            "entertainment", "game", "movie", "series", "streaming",
            "блогер", "ютуб", "тикток", "инстаграм", "знаменитость"
        }
    };

    public ClassifierService(ILogger<ClassifierService> logger)
    {
        _logger = logger;
    }

    public string Classify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Другое";

        // Убираем время и цифры из конца строки перед классификацией
        var cleanText = System.Text.RegularExpressions.Regex
            .Replace(text, @"\d{1,2}:\d{2}$", "")
            .Trim()
            .ToLowerInvariant();

        int maxScore = 0;
        string best  = "Другое";

        foreach (var category in Keywords)
        {
            int score = category.Value.Count(kw =>
                cleanText.Contains(kw, StringComparison.OrdinalIgnoreCase));

            if (score > maxScore)
            {
                maxScore = score;
                best     = category.Key;
            }
        }

        return best;
    }
}