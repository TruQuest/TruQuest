using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setTruQuestAddress")]
public class SetTruQuestAddressMessage : FunctionMessage
{
    [Parameter("address", "_truQuestAddress", 1)]
    public string TruQuestAddress { get; init; }
}
