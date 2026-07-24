using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using NewsAggregator.Core.Entities;
using NewsAggregator.Core.Interfaces;
using System.Globalization;

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
            Name            = "BBC News",
            Url             = "https://www.bbc.com/news",
            ArticleSelector = "h3[class*='title'] a, .gs-c-promo-heading a",
            DateSelector    = "time",
            BaseUrl         = "https://www.bbc.com"
        },
        new SiteConfig
        {
            Name            = "CNN",
            Url             = "https://edition.cnn.com",
            ArticleSelector = ".containerheadline a, .cdheadline a",
            DateSelector    = ".timestamp",
            BaseUrl         = "https://edition.cnn.com"
        },
        new SiteConfig
        {
            Name            = "Reuters",
            Url             = "https://www.reuters.com",
            ArticleSelector = "a[data-testid='Heading']",
            DateSelector    = "time",
            BaseUrl         = "https://www.reuters.com"
        },
        new SiteConfig
        {
            Name            = "The Guardian",
            Url             = "https://www.theguardian.com/international",
            ArticleSelector = ".fc-item__title a",
            DateSelector    = "time",
            BaseUrl         = "https://www.theguardian.com"
        },
        new SiteConfig
        {
            Name            = "Al Jazeera",
            Url             = "https://www.aljazeera.com",
            ArticleSelector = ".article-card__title a",
            DateSelector    = "time",
            BaseUrl         = "https://www.aljazeera.com"
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
        var allNews      = new List<News>();
        var proxyEnabled = _config.GetValue<bool>("ProxySettings:Enabled");
        var proxyServer  = _config.GetValue<string>("ProxySettings:Server");
        var proxyUser    = _config.GetValue<string>("ProxySettings:Username");
        var proxyPass    = _config.GetValue<string>("ProxySettings:Password");

        using var playwright = await Playwright.CreateAsync();
        var launchOptions    = new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args     = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        };

        Proxy? proxy = null;
        if (proxyEnabled && !string.IsNullOrEmpty(proxyServer))
            proxy = new Proxy { Server = proxyServer, Username = proxyUser, Password = proxyPass };

        await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);
        var contextOptions      = new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            Proxy     = proxy
        };

        await using var context = await browser.NewContextAsync(contextOptions);
        var page                = await context.NewPageAsync();
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
        var result = new List<News>();

        await page.GotoAsync(site.Url, new PageGotoOptions
        {
            Timeout   = 30000,
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        try
        {
            await page.WaitForSelectorAsync(site.ArticleSelector,
                new PageWaitForSelectorOptions { Timeout = 10000 });
        }
        catch { /* продолжаем даже если селектор не найден */ }

        await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight / 2)");
        await Task.Delay(1000);

        var links = await page.QuerySelectorAllAsync(site.ArticleSelector);

        foreach (var link in links.Take(15))
        {
            try
            {
                var href  = await link.GetAttributeAsync("href");
                var title = (await link.InnerTextAsync()).Trim();

                if (string.IsNullOrWhiteSpace(href) || title.Length < 10) continue;

                var fullUrl = href.StartsWith("http")
                    ? href
                    : site.BaseUrl + (href.StartsWith("/") ? href : "/" + href);

                var categoryName = _classifier.Classify(title);

                var news = new News
                {
                    Title         = title,
                    Url           = fullUrl,
                    Source        = site.Name,
                    Content       = title.Length > 200 ? title[..200] : title,
                    PublishedDate = DateTimeOffset.UtcNow,
                    CreatedAt     = DateTimeOffset.UtcNow,
                    CategoryId    = ResolveCategoryId(categoryName)
                };

                result.Add(news);
                await Task.Delay(Random.Shared.Next(1000, 2000));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка обработки ссылки на {Site}", site.Name);
            }
        }

        return result;
    }

    private static int ResolveCategoryId(string categoryName) => categoryName switch
    {
        "Политика"    => 1,
        "Экономика"   => 2,
        "Спорт"       => 3,
        "Технологии"  => 4,
        "Наука"       => 5,
        "Культура"    => 6,
        "Здоровье"    => 7,
        "Бизнес"      => 8,
        "Экология"    => 9,
        "Развлечения" => 10,
        _             => 13
    };

    private record SiteConfig
    {
        public string Name            { get; init; } = string.Empty;
        public string Url             { get; init; } = string.Empty;
        public string ArticleSelector { get; init; } = string.Empty;
        public string DateSelector    { get; init; } = string.Empty;
        public string BaseUrl         { get; init; } = string.Empty;
    }
}