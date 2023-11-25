enum SettlementProposalStateVm {
  draft,
  awaitingFunding,
  fundedAndVerifierLotteryInitiated,
  verifierLotteryFailed,
  verifiersSelectedAndPollInitiated,
  consensusNotReached,
  declined,
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
      case SettlementProposalStateVm.verifierLotteryFailed:
        return 'Verifier lottery failed';
      case SettlementProposalStateVm.verifiersSelectedAndPollInitiated:
        return 'Awaiting assessment poll results';
      case SettlementProposalStateVm.consensusNotReached:
        return 'Consensus not reached';
      case SettlementProposalStateVm.declined:
        return 'Declined';
      case SettlementProposalStateVm.accepted:
        return 'Accepted';
    }
  }
}
