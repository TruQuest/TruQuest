using System.Transactions;

namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class ExecuteInTxnAttribute : Attribute
{
    public IsolationLevel IsolationLevel { get; init; } = IsolationLevel.Serializable;
    public Type[]? ExcludeRepos { get; init; }
}