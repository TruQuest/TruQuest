import '../../../general/models/vm/orchestrator_lottery_commitment_vm.dart';
import '../../../general/models/vm/verifier_lottery_participant_entry_vm.dart';

class ThingValidationVerifierLotteryVm {
  final OrchestratorLotteryCommitmentVm orchestratorCommitment;
  final List<VerifierLotteryParticipantEntryVm> participants;

  ThingValidationVerifierLotteryVm.fromMap(Map<String, dynamic> map)
      : orchestratorCommitment = OrchestratorLotteryCommitmentVm.fromExportMap(map['orchestratorCommitment']),
        participants = List.unmodifiable(
          (map['participants'] as List<dynamic>)
              .map((submap) => VerifierLotteryParticipantEntryVm.fromExportMap(submap)),
        );
}
