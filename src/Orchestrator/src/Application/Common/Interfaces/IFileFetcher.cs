namespace Application.Common.Interfaces;

public interface IFileFetcher
{
    IAsyncEnumerable<string> FetchAll<T>(T input, string userId);
}