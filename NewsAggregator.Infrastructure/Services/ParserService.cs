using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NewsAggregator.Core.Entities;
using NewsAggregator.Core.Interfaces;

namespace NewsAggregator.Infrastructure.Services;

public class ParserService : IParserService
{
    private readonly ILogger<ParserService> _logger;
    private readonly IClassifierService _classifier;
    private readonly HttpClient _http;

    private static readonly RssSource[] RssSources =
    {
        new("Lenta.ru",       "https://lenta.ru/rss/",     "https://lenta.ru"),
        new("ТАСС",           "https://tass.ru/rss/v2.xml", "https://tass.ru"),
        new("RT на русском",  "https://russian.rt.com/rss", "https://russian.rt.com")
    };

    private static readonly HtmlSource[] HtmlSources =
    {
        new("РБК", "https://www.rbc.ru/", "https://www.rbc.ru",
            // Заголовок и ссылка на статью на главной РБК
            @"data-role=""title-link"" href=""([^""]+)""[^>]*>\s*<div class=""spaced-items""><span[^>]*>([^<]+)</span>",
            IsBlockPattern: false),
        new("Известия", "https://iz.ru/", "https://iz.ru",
            // Ссылка на карточку; заголовок лежит внутри самой ссылки в <span>
            @"<a href=""(/\d+/[^""]+)""\s+class=""node__cart__item__inside[^""]*""[^>]*>([\s\S]*?)</a>",
            IsBlockPattern: true)
    };

    private static readonly Regex OgImageRegex = new(
        @"<meta[^>]+(?:property|name)=['""]og:image['""][^>]+content=['""]([^'""]+)['""]",
        RegexOptions.IgnoreCase);

    public ParserService(
        ILogger<ParserService> logger,
        IClassifierService classifier)
    {
        _logger     = logger;
        _classifier = classifier;

        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression   = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            ConnectTimeout           = TimeSpan.FromSeconds(15)
        };

        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
    }

    public async Task<List<News>> ParseAllSourcesAsync()
    {
        var allNews  = new List<News>();
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var site in RssSources)
        {
            try
            {
                _logger.LogInformation("Парсинг: {Site}", site.Name);
                var news = await ParseRssAsync(site, seenUrls);
                allNews.AddRange(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка парсинга RSS {Site}", site.Name);
            }

            await Task.Delay(Random.Shared.Next(1000, 2500));
        }

        foreach (var site in HtmlSources)
        {
            try
            {
                _logger.LogInformation("Парсинг: {Site}", site.Name);
                var news = await ParseHtmlAsync(site, seenUrls);
                allNews.AddRange(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка парсинга HTML {Site}", site.Name);
            }

            await Task.Delay(Random.Shared.Next(1000, 2500));
        }

        await EnrichWithImagesAsync(allNews);

        _logger.LogInformation("Итого собрано {Count} новостей", allNews.Count);
        return allNews;
    }

    private async Task<List<News>> ParseRssAsync(RssSource site, HashSet<string> seenUrls)
    {
        var result = new List<News>();

        using var stream = await _http.GetStreamAsync(site.RssUrl);
        XDocument doc;
        try
        {
            doc = XDocument.Load(stream);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Некорректный RSS {Site}: {Error}", site.Name, ex.Message);
            return result;
        }

        var ns    = doc.Root?.Name.Namespace ?? XNamespace.None;
        var items = doc.Descendants(ns + "item").Take(30).ToList();
        _logger.LogInformation("Найдено {Count} статей в RSS {Site}", items.Count, site.Name);

        foreach (var el in items)
        {
            try
            {
                var title = (el.Element(ns + "title")?.Value ?? string.Empty).Trim();
                if (title.Length < 10) continue;

                var link = (el.Element(ns + "link")?.Value
                            ?? el.Element(ns + "guid")?.Value
                            ?? string.Empty).Trim();

                var cleanUrl = NormalizeUrl(link, site.BaseUrl);
                if (cleanUrl == null || !seenUrls.Add(cleanUrl)) continue;

                var imageUrl = el.Element(ns + "enclosure")?.Attribute("url")?.Value;
                var desc     = (el.Element(ns + "description")?.Value ?? string.Empty).Trim();

                // RT кладёт превью-картинку в описание как <img src="…"/>, ТАСС — не даёт вовсе.
                if (string.IsNullOrWhiteSpace(imageUrl))
                    imageUrl = ExtractFirstImage(desc);

                result.Add(CreateArticle(
                    title,
                    cleanUrl,
                    desc,
                    site.Name,
                    ParseDate(el.Element(ns + "pubDate")?.Value),
                    imageUrl));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Ошибка RSS-статьи: {Error}", ex.Message);
            }
        }

        return result;
    }

    private async Task<List<News>> ParseHtmlAsync(HtmlSource site, HashSet<string> seenUrls)
    {
        var result = new List<News>();

        var html    = await _http.GetStringAsync(site.PageUrl);
        var matches = Regex.Matches(html, site.TitlePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        _logger.LogInformation("Найдено {Count} ссылок на {Site}", matches.Count, site.Name);

        foreach (Match m in matches)
        {
            try
            {
                var title = site.IsBlockPattern
                    ? ExtractTitleFromBlock(m.Groups[2].Value)
                    : StripHtml(m.Groups[2].Value);

                if (title.Length < 10) continue;

                var cleanUrl = NormalizeUrl(m.Groups[1].Value, site.BaseUrl);
                if (cleanUrl == null || !seenUrls.Add(cleanUrl)) continue;

                result.Add(CreateArticle(
                    title,
                    cleanUrl,
                    title.Length > 200 ? title[..200] : title,
                    site.Name,
                    DateTime.UtcNow,
                    null));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Ошибка HTML-статьи: {Error}", ex.Message);
            }
        }

        return result;
    }

    private static string ExtractTitleFromBlock(string block)
    {
        var span = Regex.Match(block, "<span>([^<]+)</span>");
        return span.Success ? StripHtml(span.Groups[1].Value) : string.Empty;
    }

    private async Task EnrichWithImagesAsync(List<News> allNews)
    {
        var withoutImage = allNews.Where(n => string.IsNullOrWhiteSpace(n.ImageUrl)).ToList();
        if (withoutImage.Count == 0) return;

        using var semaphore = new SemaphoreSlim(8);
        var tasks = withoutImage.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                // таймаут уже короткий — og:image смотрим только у статей без превью
                item.ImageUrl = await FetchOgImageAsync(item.Url) ?? item.ImageUrl;
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks);
    }

    private async Task<string?> FetchOgImageAsync(string articleUrl)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            var html = await _http.GetStringAsync(articleUrl, cts.Token);
            var match = OgImageRegex.Match(html);
            return match.Success ? match.Groups[1].Value : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeUrl(string url, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var full = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? url
            : baseUrl + (url.StartsWith('/') ? url : "/" + url);

        full = full.Split('?')[0].TrimEnd('/');
        return string.IsNullOrWhiteSpace(full) ? null : full;
    }

    private static string StripHtml(string input) =>
        Regex.Replace(input, "<.*?>", string.Empty).Trim();

    private static string TrimHtml(string input) =>
        StripHtml(input.Length > 200 ? input[..200] : input);

    private static string? ExtractFirstImage(string html)
    {
        var match = Regex.Match(html, "<img[^>]+src=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static DateTime ParseDate(string? value) =>
        DateTimeOffset.TryParse(value, out var dto) ? dto.UtcDateTime : DateTime.UtcNow;

    private News CreateArticle(
        string title,
        string url,
        string content,
        string source,
        DateTime publishedDate,
        string? imageUrl)
    {
var categoryName = _classifier.Classify(title);
        return new News
        {
            Title         = title.Length > 300 ? title[..300] : title,
            Url           = url,
            Source        = source,
            Content       = TrimHtml(content),
            PublishedDate = publishedDate,
            CreatedAt     = DateTime.UtcNow,
            CategoryId    = ResolveCategoryId(categoryName),
            ImageUrl      = imageUrl
        };
    }

    private static int ResolveCategoryId(string name) => name switch
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
        "Образование" => 11,
        "Путешествия" => 12,
        _             => 13
    };

    private sealed record RssSource(string Name, string RssUrl, string BaseUrl);

    private sealed record HtmlSource(string Name, string PageUrl, string BaseUrl, string TitlePattern, bool IsBlockPattern);
}