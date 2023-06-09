using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("joinLottery")]
public class JoinThingAssessmentVerifierLotteryMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("bytes32", "_userData", 2)]
    public byte[] UserData { get; init; }
}