class VerifierLotteryInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int thingVerifiersArrayIndex;
  final bool? alreadyClaimedASpot;
  final bool? alreadyJoined;

  const VerifierLotteryInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.thingVerifiersArrayIndex,
    required this.alreadyClaimedASpot,
    required this.alreadyJoined,
  });
}
