import '../../general/bloc/actions.dart';
import '../../general/bloc/mixins.dart';
import 'settlement_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class SettlementAction extends Action {
  const SettlementAction();
}

abstract class SettlementActionAwaitable<T extends SettlementResultVm?>
    extends SettlementAction with AwaitableResult<T> {}

class CreateNewSettlementProposalDraft extends SettlementActionAwaitable<
    CreateNewSettlementProposalDraftFailureVm?> {
  final DocumentContext documentContext;

  CreateNewSettlementProposalDraft({required this.documentContext});
}

class GetSettlementProposal extends SettlementAction {
  final String proposalId;

  const GetSettlementProposal({required this.proposalId});
}

class SubmitNewSettlementProposal
    extends SettlementActionAwaitable<SubmitNewSettlementProposalFailureVm?> {
  final String proposalId;

  SubmitNewSettlementProposal({required this.proposalId});
}

class GetVerifierLotteryInfo extends SettlementAction {
  final String thingId;
  final String proposalId;

  const GetVerifierLotteryInfo({
    required this.thingId,
    required this.proposalId,
  });
}

class GetVerifierLotteryParticipants extends SettlementAction {
  final String thingId;
  final String proposalId;

  const GetVerifierLotteryParticipants({
    required this.thingId,
    required this.proposalId,
  });
}

class GetAssessmentPollInfo
    extends SettlementActionAwaitable<GetAssessmentPollInfoSuccessVm> {
  final String thingId;
  final String proposalId;

  GetAssessmentPollInfo({
    required this.thingId,
    required this.proposalId,
  });
}

class GetVerifiers extends SettlementAction {
  final String proposalId;

  const GetVerifiers({required this.proposalId});
}
