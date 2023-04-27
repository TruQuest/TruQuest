enum SettlementProposalStateVm {
  draft,
  awaitingFunding,
  fundedAndVerifierLotteryInitiated,
  verifiersSelectedAndPollInitiated,
  softDeclined,
  hardDeclined,
  accepted,
}

extension SettlementProposalStateVmExtension on SettlementProposalStateVm {
  String getString() {
    switch (this) {
      case SettlementProposalStateVm.draft:
        return 'Draft';
      case SettlementProposalStateVm.awaitingFunding:
        return 'Awaiting funding';
      case SettlementProposalStateVm.fundedAndVerifierLotteryInitiated:
        return 'Awaiting verifier lottery results';
      case SettlementProposalStateVm.verifiersSelectedAndPollInitiated:
        return 'Awaiting assessment poll results';
      case SettlementProposalStateVm.softDeclined:
        return 'Soft declined';
      case SettlementProposalStateVm.hardDeclined:
        return 'Hard declined';
      case SettlementProposalStateVm.accepted:
        return 'Accepted';
    }
  }
}