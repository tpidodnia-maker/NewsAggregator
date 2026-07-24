using System.Net;
using System.Text.Json;

namespace NewsAggregator.WebAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанное исключение");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentException           => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException        => (int)HttpStatusCode.NotFound,
                _                           => (int)HttpStatusCode.InternalServerError
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error   = ex.Message,
                status  = context.Response.StatusCode
            }));
        }
    }
}