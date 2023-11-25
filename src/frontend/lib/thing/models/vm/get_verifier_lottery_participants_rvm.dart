import '../../../general/models/vm/lottery_closed_event_vm.dart';
import '../../../general/models/vm/orchestrator_lottery_commitment_vm.dart';
import '../../../general/models/vm/verifier_lottery_participant_entry_vm.dart';

class GetVerifierLotteryParticipantsRvm {
  final String thingId;
  final OrchestratorLotteryCommitmentVm? orchestratorCommitment;
  final LotteryClosedEventVm? lotteryClosedEvent;
  final List<VerifierLotteryParticipantEntryVm> participants;

  GetVerifierLotteryParticipantsRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        orchestratorCommitment = map['orchestratorCommitment'] != null
            ? OrchestratorLotteryCommitmentVm.fromMap(map['orchestratorCommitment'])
            : null,
        lotteryClosedEvent =
            map['lotteryClosedEvent'] != null ? LotteryClosedEventVm.fromMap(map['lotteryClosedEvent']) : null,
        participants = List.unmodifiable(
          (map['participants'] as List<dynamic>).map(
            (submap) => VerifierLotteryParticipantEntryVm.fromMap(submap),
          ),
        );
}
