using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedNewAcceptancePollVoteTd")]
public class SignedNewAcceptancePollVoteTd
{
    [Parameter("tuple", "vote", 1, "NewAcceptancePollVoteTd")]
    public NewAcceptancePollVoteTd Vote { get; init; }
    [Parameter("string", "voterId", 2)]
    public string VoterId { get; init; }
    [Parameter("string", "voterSignature", 3)]
    public string VoterSignature { get; init; }
}