namespace Services;

internal interface IWebPageScreenshotTaker
{
    Task<List<string>?> Take(string requestId, IEnumerable<string> urls);
}
