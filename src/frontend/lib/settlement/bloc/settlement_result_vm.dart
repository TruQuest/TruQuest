abstract class SettlementResultVm {}

class CreateNewSettlementProposalDraftFailureVm extends SettlementResultVm {}

class SubmitNewSettlementProposalFailureVm extends SettlementResultVm {}

class GetVerifierLotteryInfoSuccessVm extends SettlementResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInThingVerifiersArray;
  final bool? alreadyClaimedASpot;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInThingVerifiersArray,
    required this.alreadyClaimedASpot,
    required this.alreadyJoined,
  });
}

class GetAssessmentPollInfoSuccessVm extends SettlementResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInProposalVerifiersArray;

  GetAssessmentPollInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInProposalVerifiersArray,
  });
}
