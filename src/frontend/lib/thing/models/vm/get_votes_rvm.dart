import 'thing_state_vm.dart';
import '../../../general/models/vm/vote_vm.dart';

class GetVotesRvm {
  final String thingId;
  final ThingStateVm thingState;
  final String? voteAggIpfsCid;
  final List<VoteVm> votes;

  String get decision => thingState == ThingStateVm.verifiersSelectedAndPollInitiated
      ? 'Pending'
      : thingState == ThingStateVm.consensusNotReached || thingState == ThingStateVm.declined
          ? thingState.getString()
          : 'Accepted';

  GetVotesRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        thingState = ThingStateVm.values[map['thingState']],
        voteAggIpfsCid = map['voteAggIpfsCid'],
        votes = List.unmodifiable(
          (map['votes'] as List<dynamic>).map((submap) => VoteVm.fromMap(submap)),
        );
}
