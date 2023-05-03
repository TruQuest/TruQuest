// @@??: Move to general?
enum ThingStateVm {
  draft,
  awaitingFunding,
  fundedAndVerifierLotteryInitiated,
  verifiersSelectedAndPollInitiated,
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
      case ThingStateVm.verifiersSelectedAndPollInitiated:
        return 'Awaiting acceptance poll results';
      case ThingStateVm.awaitingSettlement:
        return 'Awaiting settlement';
      case ThingStateVm.settled:
        return 'Settled';
    }
  }
}
