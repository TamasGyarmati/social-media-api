using Microsoft.AspNetCore.Hosting;

namespace SocialMedia.Logic.Services;

public record RequestLogEntry(
    DateTime TimestampUtc,
    string HttpMethod,
    string Path,
    string? QueryString,
    int StatusCode,
    long ElapsedMilliseconds,
    string? UserId,
    string? UserName,
    string? IpAddress
);

public interface IFileLogger
{
    Task LogRequestAsync(RequestLogEntry entry);
}

public class FileLogger : IFileLogger
{ 
    static readonly SemaphoreSlim _lock = new(1, 1);
    readonly string _logsDirectory;

    public FileLogger(IWebHostEnvironment env)
    {
        _logsDirectory = Path.Combine(env.ContentRootPath, "Logs");
        Directory.CreateDirectory(_logsDirectory);
    }
    
    public async Task LogRequestAsync(RequestLogEntry entry)
    {
        var fileName = $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt";
        var filePath = Path.Combine(_logsDirectory, fileName);
        
        var logLine = $"[{entry.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}] " +
                      $"{entry.HttpMethod} {entry.Path}{entry.QueryString} " +
                      $"-> Status: {entry.StatusCode} ({entry.ElapsedMilliseconds}ms) " +
                      $"| User: {entry.UserId ?? "Anonymous"} " +
                      $"| UserName: {entry.UserName ?? "Anonymous"} " +
                      $"| IP: {entry.IpAddress ?? "Unknown"}{Environment.NewLine}";

        await _lock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(filePath, logLine);
        }
        finally
        {
            _lock.Release();
        }
    }
}