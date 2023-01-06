using System.Diagnostics;

namespace Services;

internal class WebPageSaver : IWebPageSaver
{
    private readonly ILogger<WebPageSaver> _logger;

    public WebPageSaver(ILogger<WebPageSaver> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> SaveLocalCopies(IEnumerable<string> urls)
    {
        var requestId = Guid.NewGuid().ToString();

        using (var fs = new FileStream($"/singlefile/input/{requestId}.txt", FileMode.CreateNew, FileAccess.Write))
        {
            using var sw = new StreamWriter(fs);
            foreach (var url in urls)
            {
                await sw.WriteLineAsync(url);
            }
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

        try
        {
            process.Start();
        }
        catch
        {
            var dir = new DirectoryInfo($"/singlefile/output/{requestId}");
            if (dir.Exists)
            {
                dir.Delete(recursive: true);
            }
            throw;
        }

        try
        {
            // @@TODO: Figure out what happens if single-file exits with an error. Will WaitForExitAsync throw?
            // Will it return normally but with process.ExitCode != 0? Or smth else?
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
        }
        catch
        {
            var dir = new DirectoryInfo($"/singlefile/output/{requestId}");
            if (dir.Exists)
            {
                dir.Delete(recursive: true);
            }
            throw;
        }

        File.Delete($"/singlefile/input/{requestId}.txt");

        // @@??: Check that there are as many output files as there are urls?

        var filePaths = new List<string>(urls.Count());
        foreach (var url in urls)
        {
            var flatUrl = url.EndsWith('/') ? url : (url + "/");
            flatUrl = flatUrl.Replace("://", "__").Replace('/', '_').Replace('?', '_');
            filePaths.Add($"/singlefile/output/{requestId}/{flatUrl}.html");
        }

        return filePaths;
    }
}