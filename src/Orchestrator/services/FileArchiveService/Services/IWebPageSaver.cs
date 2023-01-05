namespace Services;

internal interface IWebPageSaver
{
    Task<(string htmlFilePath, string jpgFilePath)> SaveLocalCopy(string url);
}