namespace Services;

internal interface IWebPageScreenshotTaker
{
    Task<List<string>> Take(IEnumerable<string> filePaths);
}