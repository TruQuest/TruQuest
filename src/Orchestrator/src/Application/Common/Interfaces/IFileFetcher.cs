using System.Reflection;

namespace Application.Common.Interfaces;

public interface IFileFetcher
{
    IAsyncEnumerable<(string filePath, PropertyInfo prop)> FetchAll<T>(T input, string userId);
}