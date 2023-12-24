import 'poll_vm.dart';
import 'settlement_proposal_assessment_verifier_lottery_vm.dart';

class SettlementProposalVm {
  final String id;
  final String title;
  final SettlementProposalAssessmentVerifierLotteryVm? lottery;
  final PollVm? poll;

  SettlementProposalVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        title = map['title'],
        lottery = map['lottery'] != null ? SettlementProposalAssessmentVerifierLotteryVm.fromMap(map['lottery']) : null,
        poll = map['poll'] != null ? PollVm.fromMap(map['poll']) : null;
}
