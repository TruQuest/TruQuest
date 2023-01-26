abstract class SettlementResultVm {}

abstract class CreateNewSettlementProposalDraftResultVm
    extends SettlementResultVm {}

class CreateNewSettlementProposalDraftSuccessVm
    extends CreateNewSettlementProposalDraftResultVm {}

class CreateNewSettlementProposalDraftFailureVm
    extends CreateNewSettlementProposalDraftResultVm {}

class SubmitNewSettlementProposalSuccessVm extends SettlementResultVm {
  final String signature;

  SubmitNewSettlementProposalSuccessVm({required this.signature});
}
