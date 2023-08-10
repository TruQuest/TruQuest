using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("s_votingVolumeThresholdPercent", "uint8")]
public class GetPollVotingVolumeThresholdPercentMessage : FunctionMessage { }
