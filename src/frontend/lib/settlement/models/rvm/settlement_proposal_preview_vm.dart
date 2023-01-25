import 'settlement_proposal_state_vm.dart';
import 'verdict_vm.dart';

class SettlementProposalPreviewVm {
  final String id;
  final SettlementProposalStateVm state;
  final String title;
  final VerdictVm verdict;
  final String? croppedImageIpfsCid;
  final String submitterId;

  SettlementProposalPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = SettlementProposalStateVm.values[map['state']],
        title = map['title'],
        verdict = VerdictVm.values[map['verdict']],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'];
}
