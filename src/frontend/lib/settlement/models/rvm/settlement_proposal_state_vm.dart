enum SettlementProposalStateVm {
  draft,
  awaitingFunding,
  fundedAndAssessmentVerifierLotteryInitiated,
  assessmentVerifiersSelectedAndPollInitiated,
}

extension SettlementProposalStateVmExtension on SettlementProposalStateVm {
  String getString() {
    switch (this) {
      case SettlementProposalStateVm.draft:
        return 'Draft';
      case SettlementProposalStateVm.awaitingFunding:
        return 'Awaiting funding';
      case SettlementProposalStateVm
          .fundedAndAssessmentVerifierLotteryInitiated:
        return 'Awaiting verifier lottery results';
      case SettlementProposalStateVm
          .assessmentVerifiersSelectedAndPollInitiated:
        return 'Awaiting assessment poll results';
    }
  }
}
