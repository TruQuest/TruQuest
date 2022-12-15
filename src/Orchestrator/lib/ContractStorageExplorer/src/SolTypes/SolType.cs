namespace ContractStorageExplorer.SolTypes;

public abstract class SolType
{
    public abstract int SizeBits { get; }
    protected internal virtual object ValueObject { get; set; }
    public abstract string HexValue { get; }
}

public abstract class SolType<T> : SolType
{
    public abstract T Value { get; }
}