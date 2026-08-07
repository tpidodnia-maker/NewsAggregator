using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NewsAggregator.Core.Entities;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.Infrastructure.Services;

public class ParserService : IParserService
{
    private readonly ILogger<ParserService> _logger;
    private readonly IClassifierService _classifier;
    private readonly IConfiguration _config;

    private static readonly List<SiteConfig> Sites = new()
    {
        new SiteConfig
        {
            Name            = "РБК",
            Url             = "https://www.rbc.ru/",
            ArticleSelector = "a.item__link, a.search-result__item-title",
            BaseUrl         = "https://www.rbc.ru"
        },
        new SiteConfig
        {
            Name            = "Lenta.ru",
            Url             = "https://lenta.ru/",
            ArticleSelector = "a.card-full-other, a.topic-card__link",
            BaseUrl         = "https://lenta.ru"
        },
        new SiteConfig
        {
            Name            = "RT на русском",
            Url             = "https://russian.rt.com/",
            ArticleSelector = "a.link.card__heading, a[class*='card__heading']",
            BaseUrl         = "https://russian.rt.com"
        },
        new SiteConfig
        {
            Name            = "Известия",
            Url             = "https://iz.ru/",
            ArticleSelector = "a.node__cart__item__inside__info, a[href*='/news/']",
            BaseUrl         = "https://iz.ru"
        },
        new SiteConfig
        {
            Name            = "ТАСС",
            Url             = "https://tass.ru/",
            ArticleSelector = "a[href*='/politika/'], a[href*='/ekonomika/'], a[href*='/sport/'], a[href*='/nauka/']",
            BaseUrl         = "https://tass.ru"
        }
    };

    public ParserService(
        ILogger<ParserService> logger,
        IClassifierService classifier,
        IConfiguration config)
    {
        _logger     = logger;
        _classifier = classifier;
        _config     = config;
    }

    public async Task<List<News>> ParseAllSourcesAsync()
    {
        var allNews = new List<News>();

        using var playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args     = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
        };

        await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);

        var contextOptions = new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            Locale    = "ru-RU"
        };

        await using var context = await browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();

        foreach (var site in Sites)
        {
            try
            {
                _logger.LogInformation("Парсинг: {Site}", site.Name);
                var news = await ParseSiteAsync(page, site);
                allNews.AddRange(news);
                await Task.Delay(Random.Shared.Next(2000, 4000));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка парсинга {Site}", site.Name);
            }
        }

        return allNews;
    }

    private async Task<List<News>> ParseSiteAsync(IPage page, SiteConfig site)
    {
        var result   = new List<News>();
        var seenUrls = new HashSet<string>();

        try
        {
            await page.GotoAsync(site.Url, new PageGotoOptions
            {
                Timeout   = 30000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            await Task.Delay(2000);
            await page.EvaluateAsync("window.scrollTo(0, 800)");
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Не удалось загрузить {Site}: {Error}", site.Name, ex.Message);
            return result;
        }

        var links = await page.QuerySelectorAllAsync(site.ArticleSelector);
        _logger.LogInformation("Найдено {Count} ссылок на {Site}", links.Count, site.Name);

        if (links.Count == 0)
        {
            try
            {
                var debugDir = Path.Combine(AppContext.BaseDirectory, "parse-debug");
                Directory.CreateDirectory(debugDir);
                var safeSiteName = site.Name.Replace(" ", "_").Replace(".", "_");
                var pageTitle = await page.TitleAsync();
                var html = await page.ContentAsync();
                await File.WriteAllTextAsync(Path.Combine(debugDir, $"{safeSiteName}.html"), html);
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = Path.Combine(debugDir, $"{safeSiteName}.png"),
                    FullPage = false
                });
                _logger.LogWarning(
                    "ДИАГНОСТИКА {Site}: заголовок страницы = \"{Title}\", длина HTML = {Len} символов. " +
                    "Скриншот и HTML сохранены в {Dir}",
                    site.Name, pageTitle, html.Length, debugDir);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось сохранить диагностику для {Site}", site.Name);
            }
        }

        foreach (var link in links.Take(25))
        {
            try
            {
                var href  = await link.GetAttributeAsync("href") ?? "";
                var rawTitle = (await link.InnerTextAsync()).Trim();
                var title = System.Text.RegularExpressions.Regex
                .Replace(rawTitle, @"\s+\d{1,2}:\d{2}$", "")
                .Trim();

                if (string.IsNullOrWhiteSpace(href) || title.Length < 10) continue;

                var fullUrl = href.StartsWith("http")
                    ? href
                    : site.BaseUrl + (href.StartsWith("/") ? href : "/" + href);

                var cleanUrl = fullUrl.Split('?')[0].TrimEnd('/');

                if (seenUrls.Contains(cleanUrl)) continue;
                seenUrls.Add(cleanUrl);

                var categoryName = _classifier.Classify(title);
                var imageUrl     = await ExtractImageUrlAsync(link, site.BaseUrl);

                result.Add(new News
                {
                    Title         = title.Length > 300 ? title[..300] : title,
                    Url           = cleanUrl,
                    Source        = site.Name,
                    Content       = title.Length > 200 ? title[..200] : title,
                    PublishedDate = DateTime.UtcNow,
                    CreatedAt     = DateTime.UtcNow,
                    CategoryId    = ResolveCategoryId(categoryName),
                    ImageUrl      = imageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Ошибка ссылки: {Error}", ex.Message);
            }
        }

        _logger.LogInformation("Собрано {Count} новостей с {Site}", result.Count, site.Name);

        // Пытаемся заменить thumbnail-картинку с листинга на полноразмерное og:image
        // со страницы самой статьи — лёгкие HTTP-запросы, без headless-браузера.
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var semaphore = new SemaphoreSlim(5);
        var enrichTasks = result.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                var ogImage = await FetchOgImageAsync(http, item.Url);
                if (!string.IsNullOrWhiteSpace(ogImage))
                    item.ImageUrl = ogImage;
            }
            finally { semaphore.Release(); }
        });
        await Task.WhenAll(enrichTasks);

        return result;
    }

    private static readonly System.Text.RegularExpressions.Regex OgImageRegex =
        new(@"<meta[^>]+property=[""']og:image[""'][^>]+content=[""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private static async Task<string?> FetchOgImageAsync(HttpClient http, string articleUrl)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            var html = await http.GetStringAsync(articleUrl, cts.Token);
            var match = OgImageRegex.Match(html);
            return match.Success ? match.Groups[1].Value : null;
        }
        catch
        {
            return null; // не страшно — останется thumbnail с листинга
        }
    }

    /// <summary>
    /// Пытается найти превью-картинку рядом со ссылкой на статью прямо на странице листинга
    /// (без захода на саму статью — это было бы слишком дорого по времени).
    /// Ищет &lt;img&gt; внутри самой ссылки, затем — в ближайшем родительском блоке карточки.
    /// Возвращает null, если картинки нет — тогда фронтенд покажет плейсхолдер.
    /// </summary>
    private static async Task<string?> ExtractImageUrlAsync(IElementHandle link, string baseUrl)
    {
        try
        {
            var img = await link.QuerySelectorAsync("img");

            if (img == null)
            {
                var parent = await link.EvaluateHandleAsync(
                    "el => el.closest('article, li, div')") as IElementHandle;
                if (parent != null)
                    img = await parent.QuerySelectorAsync("img");
            }

            if (img == null) return null;

            var src = await img.GetAttributeAsync("data-src")
                      ?? await img.GetAttributeAsync("src");

            if (string.IsNullOrWhiteSpace(src) || src.StartsWith("data:")) return null;

            return src.StartsWith("http") ? src : baseUrl + (src.StartsWith("/") ? src : "/" + src);
        }
        catch
        {
            return null;
        }
    }

    private static int ResolveCategoryId(string name) => name switch
    {
        "Политика"     => 1,
        "Экономика"    => 2,
        "Спорт"        => 3,
        "Технологии"   => 4,
        "Наука"        => 5,
        "Культура"     => 6,
        "Здоровье"     => 7,
        "Бизнес"       => 8,
        "Экология"     => 9,
        "Развлечения"  => 10,
        "Образование"  => 11,
        "Путешествия"  => 12,
        _              => 13
    };

    private record SiteConfig
    {
        public string Name            { get; init; } = string.Empty;
        public string Url             { get; init; } = string.Empty;
        public string ArticleSelector { get; init; } = string.Empty;
        public string BaseUrl         { get; init; } = string.Empty;
    }
}