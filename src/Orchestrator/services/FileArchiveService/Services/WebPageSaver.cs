using System.Diagnostics;

namespace Services;

internal class WebPageSaver : IWebPageSaver
{
    private readonly ILogger<WebPageSaver> _logger;

    public WebPageSaver(ILogger<WebPageSaver> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveLocalCopy(string url)
    {
        var requestId = Guid.NewGuid().ToString();

        using (var fs = new FileStream($"/singlefile/input/{requestId}.txt", FileMode.CreateNew, FileAccess.Write))
        {
            using var sw = new StreamWriter(fs);
            await sw.WriteLineAsync(url);
            await sw.FlushAsync();
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"/singlefile/singlefile.sh {requestId}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();

        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync()) != null)
        {
            _logger.LogInformation(line);
        }
        while ((line = await process.StandardError.ReadLineAsync()) != null)
        {
            _logger.LogWarning(line);
        }

        await process.WaitForExitAsync();

        url = url.EndsWith('/') ? url : (url + "/");

        return $"/singlefile/output/{requestId}/{url.Replace("://", "__").Replace('/', '_').Replace('?', '_')}.html";
    }
}