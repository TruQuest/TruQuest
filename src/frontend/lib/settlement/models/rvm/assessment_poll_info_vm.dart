class AssessmentPollInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int settlementProposalVerifiersArrayIndex;

  const AssessmentPollInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.settlementProposalVerifiersArrayIndex,
  });
}
