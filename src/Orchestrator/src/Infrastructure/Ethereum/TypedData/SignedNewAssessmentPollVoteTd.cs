using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.TypedData;

[Struct("SignedNewAssessmentPollVoteTd")]
public class SignedNewAssessmentPollVoteTd
{
    [Parameter("tuple", "vote", 1, "NewAssessmentPollVoteTd")]
    public NewAssessmentPollVoteTd Vote { get; init; }
    [Parameter("string", "voterId", 2)]
    public string VoterId { get; init; }
    [Parameter("string", "voterSignature", 3)]
    public string VoterSignature { get; init; }
}