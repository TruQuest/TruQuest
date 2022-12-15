namespace ContractStorageExplorer.SolTypes;

public abstract class SolValueType : SolType { }

public abstract class SolValueType<T> : SolValueType
{
    public abstract T Value { get; }
}