namespace Services;

internal interface IWebPageSaver
{
    Task<string> SaveLocalCopy(string url);
}