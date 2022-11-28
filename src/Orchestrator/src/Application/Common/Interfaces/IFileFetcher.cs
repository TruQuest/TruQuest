using System.Reflection;

namespace Application.Common.Interfaces;

public interface IFileFetcher
{
    IAsyncEnumerable<(string filePath, object obj, PropertyInfo prop)> FetchAll(object input, string userId);
}