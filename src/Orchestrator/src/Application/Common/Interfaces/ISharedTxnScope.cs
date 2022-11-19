using System.Data.Common;

namespace Application.Common.Interfaces;

public interface ISharedTxnScope : IDisposable
{
    DbConnection? DbConnection { get; }
    HashSet<Type>? ExcludeRepos { get; }
    void Init(Type[]? excludeRepos);
}