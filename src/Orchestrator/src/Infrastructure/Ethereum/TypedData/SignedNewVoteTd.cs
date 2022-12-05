using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedNewVoteTd")]
public class SignedNewVoteTd
{
    [Parameter("tuple", "vote", 1, "NewVoteTd")]
    public NewVoteTd Vote { get; init; }
    [Parameter("string", "voterId", 2)]
    public string VoterId { get; init; }
    [Parameter("string", "voterSignature", 3)]
    public string VoterSignature { get; init; }
}