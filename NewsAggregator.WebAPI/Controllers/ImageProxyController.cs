using Microsoft.AspNetCore.Mvc;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ImageProxyController> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml", "image/avif"
    };

    public ImageProxyController(IHttpClientFactory httpClientFactory, ILogger<ImageProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            return BadRequest("Некорректный url");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ImageProxy");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Ключевая часть: подставляем Referer/User-Agent самого сайта-источника,
            // чтобы обойти защиту от hotlinking на новостных сайтах
            request.Headers.TryAddWithoutValidation("Referer", $"{uri.Scheme}://{uri.Host}/");
            request.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            if (!AllowedContentTypes.Contains(contentType))
            {
                return BadRequest("Недопустимый тип содержимого");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            Response.Headers.CacheControl = "public, max-age=86400";
            return File(bytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось проксировать изображение {Url}", url);
            return NotFound();
        }
    }
}
