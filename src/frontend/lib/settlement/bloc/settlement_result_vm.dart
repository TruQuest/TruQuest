abstract class SettlementResultVm {}

class CreateNewSettlementProposalDraftFailureVm extends SettlementResultVm {}

class SubmitNewSettlementProposalFailureVm extends SettlementResultVm {}

class FundSettlementProposalFailureVm extends SettlementResultVm {}

class GetVerifierLotteryInfoSuccessVm extends SettlementResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInThingVerifiersArray;
  final int latestL1BlockNumber;
  final bool? alreadyClaimedASpot;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInThingVerifiersArray,
    required this.latestL1BlockNumber,
    required this.alreadyClaimedASpot,
    required this.alreadyJoined,
  });
}

class GetAssessmentPollInfoSuccessVm extends SettlementResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final bool? isDesignatedVerifier;
  final int latestL1BlockNumber;

  GetAssessmentPollInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.isDesignatedVerifier,
    required this.latestL1BlockNumber,
  });
}

class ClaimLotterySpotFailureVm extends SettlementResultVm {}

class JoinLotteryFailureVm extends SettlementResultVm {}

class CastVoteResultVm extends SettlementResultVm {}
