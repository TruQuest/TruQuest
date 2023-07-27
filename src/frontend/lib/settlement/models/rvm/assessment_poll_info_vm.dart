class AssessmentPollInfoVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInProposalVerifiersArray;

  const AssessmentPollInfoVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInProposalVerifiersArray,
  });
}
