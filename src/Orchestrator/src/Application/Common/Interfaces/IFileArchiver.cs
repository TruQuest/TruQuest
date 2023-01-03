using System.Reflection;

namespace Application.Common.Interfaces;

public interface IFileArchiver
{
    IAsyncEnumerable<(string ipfsCid, object obj, PropertyInfo prop)> ArchiveAll(object input, string userId);
}