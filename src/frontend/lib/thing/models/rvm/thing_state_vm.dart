enum ThingStateVm {
  draft,
  awaitingFunding,
  fundedAndSubmissionVerifierLotteryInitiated,
  submissionVerifiersSelectedAndPollInitiated,
  awaitingSettlement,
  settlementProposalFundedAndAssessmentVerifierLotteryInitiated,
  settlementProposalAssessmentVerifiersSelectedAndPollInitiated,
}

extension ThingStateVmExtension on ThingStateVm {
  String getString() {
    switch (this) {
      case ThingStateVm.draft:
        return 'Draft';
      case ThingStateVm.awaitingFunding:
        return 'Awaiting funding';
      case ThingStateVm.fundedAndSubmissionVerifierLotteryInitiated:
        return 'Awaiting verifier lottery results';
      case ThingStateVm.submissionVerifiersSelectedAndPollInitiated:
        return 'Awaiting acceptance poll results';
      case ThingStateVm.awaitingSettlement:
        return 'Awaiting settlement';
      case ThingStateVm
          .settlementProposalFundedAndAssessmentVerifierLotteryInitiated:
        return 'Settlement proposal under review';
      case ThingStateVm
          .settlementProposalAssessmentVerifiersSelectedAndPollInitiated:
        return 'Settlement proposal under review';
    }
  }
}
