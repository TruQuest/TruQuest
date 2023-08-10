using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages;

[Function("s_majorityThresholdPercent", "uint8")]
public class GetPollMajorityThresholdPercentMessage : FunctionMessage { }
