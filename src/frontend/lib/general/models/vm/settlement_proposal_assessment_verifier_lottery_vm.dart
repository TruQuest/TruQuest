import 'orchestrator_lottery_commitment_vm.dart';
import 'verifier_lottery_participant_entry_vm.dart';

class SettlementProposalAssessmentVerifierLotteryVm {
  final OrchestratorLotteryCommitmentVm orchestratorCommitment;
  final List<VerifierLotteryParticipantEntryVm> claimants;
  final List<VerifierLotteryParticipantEntryVm> participants;

  SettlementProposalAssessmentVerifierLotteryVm.fromMap(Map<String, dynamic> map)
      : orchestratorCommitment = OrchestratorLotteryCommitmentVm.fromExportMap(map['orchestratorCommitment']),
        claimants = List.unmodifiable(
          (map['claimants'] as List<dynamic>).map((submap) => VerifierLotteryParticipantEntryVm.fromExportMap(submap)),
        ),
        participants = List.unmodifiable(
          (map['participants'] as List<dynamic>)
              .map((submap) => VerifierLotteryParticipantEntryVm.fromExportMap(submap)),
        );
}
