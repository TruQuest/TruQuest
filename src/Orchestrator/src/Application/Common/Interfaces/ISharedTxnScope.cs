using System.Data.Common;

namespace Application.Common.Interfaces;

public interface ISharedTxnScope : IDisposable
{
    DbConnection? DbConnection { get; }
    void Init();
}
