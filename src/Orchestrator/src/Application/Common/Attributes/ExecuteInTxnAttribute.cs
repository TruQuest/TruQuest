using System.Transactions;

namespace Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ExecuteInTxnAttribute : Attribute
{
    public IsolationLevel IsolationLevel { get; init; } = IsolationLevel.Serializable;
}
