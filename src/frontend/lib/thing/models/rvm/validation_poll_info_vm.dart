class ValidationPollInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int thingVerifiersArrayIndex;

  const ValidationPollInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.thingVerifiersArrayIndex,
  });
}
