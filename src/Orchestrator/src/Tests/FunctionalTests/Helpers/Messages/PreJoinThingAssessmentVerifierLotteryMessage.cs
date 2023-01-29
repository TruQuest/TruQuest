using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("preJoinLottery")]
public class PreJoinThingAssessmentVerifierLotteryMessage : FunctionMessage
{
    [Parameter("bytes32", "_thingProposalId", 1)]
    public byte[] ThingProposalId { get; init; }
    [Parameter("bytes32", "_dataHash", 2)]
    public byte[] DataHash { get; init; }
}