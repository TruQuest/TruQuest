using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Tests.FunctionalTests.Helpers.Messages;

[Function("getLotteryDurationBlocks", "uint16")]
public class GetLotteryDurationBlocksMessage : FunctionMessage { }