import '../../../general/models/rvm/lottery_closed_event_vm.dart';
import '../../../general/models/rvm/orchestrator_lottery_commitment_vm.dart';
import '../../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';

class GetVerifierLotteryParticipantsRvm {
  final String proposalId;
  final OrchestratorLotteryCommitmentVm? orchestratorCommitment;
  final LotteryClosedEventVm? lotteryClosedEvent;
  final List<VerifierLotteryParticipantEntryVm> participants;
  final List<VerifierLotteryParticipantEntryVm> claimants;

  GetVerifierLotteryParticipantsRvm.fromMap(Map<String, dynamic> map)
      : proposalId = map['proposalId'],
        orchestratorCommitment = map['orchestratorCommitment'] != null
            ? OrchestratorLotteryCommitmentVm.fromMap(map['orchestratorCommitment'])
            : null,
        lotteryClosedEvent =
            map['lotteryClosedEvent'] != null ? LotteryClosedEventVm.fromMap(map['lotteryClosedEvent']) : null,
        participants = List.unmodifiable(
          (map['participants'] as List<dynamic>).map(
            (submap) => VerifierLotteryParticipantEntryVm.fromMap(submap),
          ),
        ),
        claimants = List.unmodifiable(
          (map['claimants'] as List<dynamic>).map(
            (submap) => VerifierLotteryParticipantEntryVm.fromMap(submap),
          ),
        );
}
