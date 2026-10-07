using System.Diagnostics;
using System.Security.Claims;
using SocialMedia.Logic.Services;

namespace SocialMedia.App.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate _next) // RequestDelegate: the next middleware, the API controller
{
    // Invoke or InvokeAsync naming needed
    public async Task InvokeAsync(HttpContext _context, IFileLogger _logger)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(_context);
        }
        finally
        {
            stopwatch.Stop();

            var entry = new RequestLogEntry(
                TimestampUtc: DateTime.UtcNow,
                HttpMethod: _context.Request.Method,
                Path: _context.Request.Path,
                QueryString: _context.Request.QueryString.ToString(),
                ElapsedMilliseconds: stopwatch.ElapsedMilliseconds,
                StatusCode: _context.Response.StatusCode,
                UserId: _context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName: _context.User.FindFirstValue(ClaimTypes.Name),
                IpAddress: _context.Connection.RemoteIpAddress?.ToString()
            );

            // Fire-and-forget: I/O is slow, don't await, make it a background service on a background thread
            // Cons: exceptions go unnoticed
            _ = _logger.LogRequestAsync(entry);
        }
    }
}