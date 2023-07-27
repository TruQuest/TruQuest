class AcceptancePollInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInThingVerifiersArray;

  const AcceptancePollInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInThingVerifiersArray,
  });
}
