using Microsoft.Playwright;

using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class PlaywrightWebPageScreenshotTaker : IWebPageScreenshotTaker
{
    public async Task Take(string url, string filePath)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

        var page = await browser.NewPageAsync(new()
        {
            ViewportSize = new() { Width = 1920, Height = 1080 }
        });
        await page.GotoAsync(url);

        await page.ScreenshotAsync(new()
        {
            Path = filePath,
            FullPage = true,
            Timeout = 30000
        });
    }
}