namespace ContractStorageExplorer.SolTypes;

public class SolBool : SolValueType<bool>
{
    public override int SizeBits => 8;

    public override bool Value => (bool)ValueObject;

    public override string HexValue => Value ? "01" : "00";

    public SolBool() { }

    public SolBool(bool value)
    {
        ValueObject = value;
    }
}