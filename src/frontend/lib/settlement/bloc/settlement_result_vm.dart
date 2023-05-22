abstract class SettlementResultVm {}

class CreateNewSettlementProposalDraftFailureVm extends SettlementResultVm {}

class SubmitNewSettlementProposalFailureVm extends SettlementResultVm {}

class FundSettlementProposalFailureVm extends SettlementResultVm {}

class GetVerifierLotteryInfoSuccessVm extends SettlementResultVm {
  final int? initBlock;
  final int durationBlocks;
  final int latestBlockNumber;
  final bool? alreadyClaimedASpot;
  final bool? alreadyPreJoined;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.initBlock,
    required this.durationBlocks,
    required this.latestBlockNumber,
    required this.alreadyClaimedASpot,
    required this.alreadyPreJoined,
    required this.alreadyJoined,
  });
}

class GetAssessmentPollInfoSuccessVm extends SettlementResultVm {
  final int? initBlock;
  final int durationBlocks;
  final bool? isDesignatedVerifier;
  final int latestBlockNumber;

  GetAssessmentPollInfoSuccessVm({
    required this.initBlock,
    required this.durationBlocks,
    required this.isDesignatedVerifier,
    required this.latestBlockNumber,
  });
}

class ClaimLotterySpotFailureVm extends SettlementResultVm {}

class PreJoinLotteryFailureVm extends SettlementResultVm {}

class JoinLotteryFailureVm extends SettlementResultVm {}

class CastVoteResultVm extends SettlementResultVm {}
