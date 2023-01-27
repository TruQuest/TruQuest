abstract class SettlementResultVm {}

abstract class CreateNewSettlementProposalDraftResultVm
    extends SettlementResultVm {}

class CreateNewSettlementProposalDraftSuccessVm
    extends CreateNewSettlementProposalDraftResultVm {}

class CreateNewSettlementProposalDraftFailureVm
    extends CreateNewSettlementProposalDraftResultVm {}

class SubmitNewSettlementProposalSuccessVm extends SettlementResultVm {}

class FundSettlementProposalSuccessVm extends SettlementResultVm {}

class GetVerifierLotteryInfoSuccessVm extends SettlementResultVm {
  final int? initBlock;
  final int durationBlocks;
  final int latestBlockNumber;
  final bool? alreadyPreJoined;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.initBlock,
    required this.durationBlocks,
    required this.latestBlockNumber,
    required this.alreadyPreJoined,
    required this.alreadyJoined,
  });
}
