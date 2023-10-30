// @@??: Move to general?
enum ThingStateVm {
  draft,
  awaitingFunding,
  fundedAndVerifierLotteryInitiated,
  verifierLotteryFailed,
  verifiersSelectedAndPollInitiated,
  consensusNotReached,
  declined,
  awaitingSettlement,
  settled,
}

extension ThingStateVmExtension on ThingStateVm {
  String getString() {
    switch (this) {
      case ThingStateVm.draft:
        return 'Draft';
      case ThingStateVm.awaitingFunding:
        return 'Awaiting funding';
      case ThingStateVm.fundedAndVerifierLotteryInitiated:
        return 'Awaiting verifier lottery results';
      case ThingStateVm.verifierLotteryFailed:
        return 'Verifier lottery failed';
      case ThingStateVm.verifiersSelectedAndPollInitiated:
        return 'Awaiting validation poll results';
      case ThingStateVm.consensusNotReached:
        return 'Consensus not reached';
      case ThingStateVm.declined:
        return 'Declined';
      case ThingStateVm.awaitingSettlement:
        return 'Awaiting settlement';
      case ThingStateVm.settled:
        return 'Settled';
    }
  }
}
