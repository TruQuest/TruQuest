import 'settlement_proposal_state_vm.dart';
import '../../../general/models/vm/vote_vm.dart';

class GetVotesRvm {
  final String proposalId;
  final SettlementProposalStateVm proposalState;
  final String? voteAggIpfsCid;
  final List<VoteVm> votes;

  String get decision => proposalState == SettlementProposalStateVm.verifiersSelectedAndPollInitiated
      ? 'Pending'
      : proposalState.getString();

  GetVotesRvm.fromMap(Map<String, dynamic> map)
      : proposalId = map['proposalId'],
        proposalState = SettlementProposalStateVm.values[map['proposalState']],
        voteAggIpfsCid = map['voteAggIpfsCid'],
        votes = List.unmodifiable(
          (map['votes'] as List<dynamic>).map((submap) => VoteVm.fromMap(submap)),
        );
}
