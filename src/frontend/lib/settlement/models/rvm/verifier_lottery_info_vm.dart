class VerifierLotteryInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInThingVerifiersArray;
  final bool? alreadyClaimedASpot;
  final bool? alreadyJoined;

  const VerifierLotteryInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInThingVerifiersArray,
    required this.alreadyClaimedASpot,
    required this.alreadyJoined,
  });
}
