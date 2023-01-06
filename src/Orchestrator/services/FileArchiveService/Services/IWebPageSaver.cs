namespace Services;

internal interface IWebPageSaver
{
    Task<List<string>> SaveLocalCopies(IEnumerable<string> urls);
}