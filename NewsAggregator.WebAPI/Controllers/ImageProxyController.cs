using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using IoFile = System.IO.File;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageProxyController> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml", "image/avif",
        "application/octet-stream" // РЅРµРєРѕС‚РѕСЂС‹Рµ CDN РЅРµ РѕС‚РґР°СЋС‚ С‚РёРї вЂ” РѕРїСЂРµРґРµР»СЏРµРј РїРѕ СЃРёРіРЅР°С‚СѓСЂРµ РЅРёР¶Рµ
    };

    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(7);

    public ImageProxyController(
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env,
        ILogger<ImageProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _env               = env;
        _logger            = logger;
    }

    private string CacheRoot
    {
        get
        {
            var dir = Path.Combine(_env.ContentRootPath, "image-cache");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            return BadRequest("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ url");
        }

        var hash  = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));
        var cfile = Path.Combine(CacheRoot, hash + ".img");
        var metaf = Path.Combine(CacheRoot, hash + ".meta");

        // 1) РљСЌС€: РѕС‚РґР°С‘Рј СЃСЂР°Р·Сѓ, РµСЃР»Рё С„Р°Р№Р» РµСЃС‚СЊ Рё РЅРµ СЃС‚Р°СЂС€Рµ TTL
        if (IoFile.Exists(cfile) && IoFile.Exists(metaf) &&
            DateTime.UtcNow - IoFile.GetLastWriteTimeUtc(cfile) < CacheTtl)
        {
            var cachedType = await IoFile.ReadAllTextAsync(metaf);
            if (!string.IsNullOrWhiteSpace(cachedType))
            {
                Response.Headers.CacheControl = "public, max-age=2592000";
                return File(IoFile.OpenRead(cfile), cachedType);
            }
        }

        byte[] bytes        = Array.Empty<byte>();
        string resolvedType = string.Empty;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ImageProxy");
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);

                // РћР±С…РѕРґРёРј Р·Р°С‰РёС‚Сѓ РѕС‚ hotlinking: Referer/UA СЃР°РјРѕРіРѕ СЃР°Р№С‚Р°-РёСЃС‚РѕС‡РЅРёРєР°
                request.Headers.TryAddWithoutValidation("Referer", $"{uri.Scheme}://{uri.Host}/");
                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                request.Headers.TryAddWithoutValidation("Accept", "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    if (attempt == 0) { await Task.Delay(300); continue; }
                    return NotFound();
                }

                var contentType = response.Content.Headers.ContentType?.MediaType
                                  ?? "application/octet-stream";
                if (!AllowedContentTypes.Contains(contentType))
                {
                    return BadRequest("РќРµРґРѕРїСѓСЃС‚РёРјС‹Р№ С‚РёРї СЃРѕРґРµСЂР¶РёРјРѕРіРѕ");
                }

                using var mem = new MemoryStream();
                await response.Content.CopyToAsync(mem);
                bytes = mem.ToArray();
                if (bytes.Length == 0) return NotFound();

                // РЈС‚РѕС‡РЅСЏРµРј С‚РёРї РїРѕ СЃРёРіРЅР°С‚СѓСЂРµ, РµСЃР»Рё СЃРµСЂРІРµСЂ РЅРµ РѕС‚РґР°Р» РЅР°СЃС‚РѕСЏС‰РёР№ image/*
                resolvedType = SniffContentType(bytes, contentType, url);
                break;
            }
            catch (Exception ex) when (attempt == 0)
            {
                _logger.LogWarning(ex, "Р РµС‚СЂР°Р№ РїСЂРѕРєСЃРёСЂРѕРІР°РЅРёСЏ {Url} (РїРѕРїС‹С‚РєР° 1/2)", url);
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ РїСЂРѕРєСЃРёСЂРѕРІР°С‚СЊ РёР·РѕР±СЂР°Р¶РµРЅРёРµ {Url}", url);
                return NotFound();
            }
        }

        if (bytes.Length == 0) return NotFound();

        // 2) РЎРѕС…СЂР°РЅСЏРµРј РІ РєСЌС€ (Р°СЃРёРЅС…СЂРѕРЅРЅРѕ, РѕС€РёР±РєРё РЅРµ РєСЂРёС‚РёС‡РЅС‹)
        try
        {
            await IoFile.WriteAllBytesAsync(cfile, bytes);
            await IoFile.WriteAllTextAsync(metaf, resolvedType);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ Р·Р°РїРёСЃР°С‚СЊ РєСЌС€ РёР·РѕР±СЂР°Р¶РµРЅРёСЏ");
        }

        Response.Headers.CacheControl = "public, max-age=2592000";
        return File(bytes, resolvedType);
    }

    private static string SniffContentType(byte[] bytes, string fallback, string path)
    {
        if (bytes.Length >= 12 &&
            Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP")
            return "image/webp";

        if (bytes.Length >= 4)
        {
            var sig = Convert.ToHexString(bytes.AsSpan(0, 4)).ToUpperInvariant();
            if (sig.StartsWith("FFD8FF", StringComparison.OrdinalIgnoreCase)) return "image/jpeg";
            if (sig.StartsWith("89504E47", StringComparison.OrdinalIgnoreCase)) return "image/png";
            if (sig.StartsWith("47494638", StringComparison.OrdinalIgnoreCase)) return "image/gif";
            if (sig.StartsWith("52494646", StringComparison.OrdinalIgnoreCase)) return "image/webp";
        }

        if (AllowedContentTypes.Contains(fallback)) return fallback;

        return path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" :
               path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ? "image/gif" :
               path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ? "image/webp" :
               path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ? "image/svg+xml" :
               path.EndsWith(".avif", StringComparison.OrdinalIgnoreCase) ? "image/avif" :
               "image/jpeg";
    }
}


