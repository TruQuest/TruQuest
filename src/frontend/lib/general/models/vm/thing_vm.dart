import 'poll_vm.dart';
import 'settlement_proposal_vm.dart';
import 'thing_validation_verifier_lottery_vm.dart';

class ThingVm {
  final String id;
  final String title;
  final ThingValidationVerifierLotteryVm? lottery;
  final PollVm? poll;
  final SettlementProposalVm? settlementProposal;

  ThingVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        title = map['title'],
        lottery = map['lottery'] != null ? ThingValidationVerifierLotteryVm.fromMap(map['lottery']) : null,
        poll = map['poll'] != null ? PollVm.fromMap(map['poll']) : null,
        settlementProposal =
            map['settlementProposal'] != null ? SettlementProposalVm.fromMap(map['settlementProposal']) : null;
}
