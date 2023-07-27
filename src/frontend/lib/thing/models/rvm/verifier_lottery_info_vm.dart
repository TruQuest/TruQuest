class VerifierLotteryInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final bool? alreadyJoined;

  const VerifierLotteryInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.alreadyJoined,
  });
}
