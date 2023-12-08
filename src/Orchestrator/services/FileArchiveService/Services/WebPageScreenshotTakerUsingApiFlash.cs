namespace Services;

internal class WebPageScreenshotTakerUsingApiFlash : IWebPageScreenshotTaker
{
    private readonly ILogger<WebPageScreenshotTakerUsingApiFlash> _logger;
    private readonly IImageSaver _imageSaver;

    private readonly string _apiFlashUrl;
    private readonly string _apiFlashAccessKey;

    public WebPageScreenshotTakerUsingApiFlash(
        IConfiguration configuration,
        ILogger<WebPageScreenshotTakerUsingApiFlash> logger,
        IImageSaver imageSaver
    )
    {
        _logger = logger;
        _imageSaver = imageSaver;

        _apiFlashUrl = configuration["WebPageScreenshots:ApiFlash:Url"]!;
        _apiFlashAccessKey = configuration["WebPageScreenshots:ApiFlash:AccessKey"]!;
    }

    public async Task<List<string>?> Take(string requestId, IEnumerable<string> urls)
    {
        var fullUrls = new List<string>(urls.Count());
        foreach (var url in urls)
        {
            var @params = new Dictionary<string, string>
            {
                {"access_key", _apiFlashAccessKey},
                {"url", url},
                {"full_page", "true"},
                {"quality", "100"},
                {"scroll_page", "true"},
                {"no_cookie_banners", "true"},
                {"no_ads", "true"},
                {"no_tracking", "true"}
            };

            var queryString = await new FormUrlEncodedContent(@params).ReadAsStringAsync();
            fullUrls.Add($"{_apiFlashUrl}?{queryString}");
        }

        var tasks = fullUrls.Select(url => _imageSaver.SaveLocalCopy(requestId, url, isWebPageScreenshot: true));

        List<string>? filePaths = null;
        try
        {
            filePaths = (await Task.WhenAll(tasks)).ToList();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error trying to take webpage screenshots");
            foreach (var task in tasks)
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    var filePath = await task;
                    File.Delete(filePath);
                }
            }
        }

        return filePaths;
    }
}
