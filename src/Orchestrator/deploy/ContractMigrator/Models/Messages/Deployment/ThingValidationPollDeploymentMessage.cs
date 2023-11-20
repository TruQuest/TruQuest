using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

public class ThingValidationPollDeploymentMessage : ContractDeploymentMessage
{
    public static string Bytecode { get; set; }

    [Parameter("address", "_truQuestAddress", 1)]
    public string TruQuestAddress { get; init; }
    [Parameter("uint16", "_durationBlocks", 2)]
    public int DurationBlocks { get; init; }
    [Parameter("uint8", "_votingVolumeThresholdPercent", 3)]
    public int VotingVolumeThresholdPercent { get; init; }
    [Parameter("uint8", "_majorityThresholdPercent", 4)]
    public int MajorityThresholdPercent { get; init; }

    public ThingValidationPollDeploymentMessage() : base(Bytecode) { }
}
