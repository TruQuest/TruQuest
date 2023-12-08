namespace Services;

internal interface IImageSaver
{
    Task<string> SaveLocalCopy(string requestId, string url, bool isWebPageScreenshot = false);
}
