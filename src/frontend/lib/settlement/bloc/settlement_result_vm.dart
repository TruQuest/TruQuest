import '../../general/errors/error.dart';

abstract class SettlementResultVm {}

class CreateNewSettlementProposalDraftFailureVm extends SettlementResultVm {}

class SubmitNewSettlementProposalFailureVm extends SettlementResultVm {}

class FundSettlementProposalFailureVm extends SettlementResultVm {
  final Error error;

  FundSettlementProposalFailureVm({required this.error});
}

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

class ClaimLotterySpotFailureVm extends SettlementResultVm {
  final Error error;

  ClaimLotterySpotFailureVm({required this.error});
}

class JoinLotteryFailureVm extends SettlementResultVm {
  final Error error;

  JoinLotteryFailureVm({required this.error});
}

class CastVoteOffChainFailureVm extends SettlementResultVm {
  final Error error;

  CastVoteOffChainFailureVm({required this.error});
}

class CastVoteOnChainFailureVm extends SettlementResultVm {
  final Error error;

  CastVoteOnChainFailureVm({required this.error});
}
