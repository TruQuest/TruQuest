using Microsoft.Playwright;

namespace Services;

internal class WebPageScreenshotTaker : IWebPageScreenshotTaker
{
    public async Task<List<string>> Take(IEnumerable<string> filePaths)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            ExecutablePath = "/usr/bin/google-chrome",
            Headless = true
        });

        var previewImageFilePaths = new List<string>(filePaths.Count());
        var page = await browser.NewPageAsync();
        foreach (var filePath in filePaths)
        {
            await page.GotoAsync("file://" + filePath);

            var previewImageFilePath = filePath.Substring(0, filePath.Length - 4) + "jpg";
            await page.ScreenshotAsync(new()
            {
                Path = previewImageFilePath,
                FullPage = false,
                Timeout = 5000
            });

            previewImageFilePaths.Add(previewImageFilePath);
        }

        return previewImageFilePaths;
    }
}