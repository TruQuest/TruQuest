import '../../general/bloc/mixins.dart';
import '../models/im/decision_im.dart';
import 'settlement_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class SettlementAction {}

abstract class SettlementActionAwaitable<T extends SettlementResultVm?>
    extends SettlementAction with AwaitableResult<T> {}

class CreateNewSettlementProposalDraft extends SettlementActionAwaitable<
    CreateNewSettlementProposalDraftFailureVm?> {
  final DocumentContext documentContext;

  CreateNewSettlementProposalDraft({required this.documentContext});
}

class GetSettlementProposal extends SettlementAction {
  final String proposalId;

  GetSettlementProposal({required this.proposalId});
}

class SubmitNewSettlementProposal
    extends SettlementActionAwaitable<SubmitNewSettlementProposalFailureVm?> {
  final String proposalId;

  SubmitNewSettlementProposal({required this.proposalId});
}

class FundSettlementProposal
    extends SettlementActionAwaitable<FundSettlementProposalFailureVm?> {
  final String thingId;
  final String proposalId;
  final String signature;

  FundSettlementProposal({
    required this.thingId,
    required this.proposalId,
    required this.signature,
  });
}

class GetVerifierLotteryInfo extends SettlementAction {
  final String thingId;
  final String proposalId;

  GetVerifierLotteryInfo({
    required this.thingId,
    required this.proposalId,
  });
}

class ClaimLotterySpot
    extends SettlementActionAwaitable<ClaimLotterySpotFailureVm?> {
  final String thingId;
  final String proposalId;
  final int userIndexInThingVerifiersArray;

  ClaimLotterySpot({
    required this.thingId,
    required this.proposalId,
    required this.userIndexInThingVerifiersArray,
  });
}

class JoinLottery extends SettlementActionAwaitable<JoinLotteryFailureVm?> {
  final String thingId;
  final String proposalId;

  JoinLottery({
    required this.thingId,
    required this.proposalId,
  });
}

class GetVerifierLotteryParticipants extends SettlementAction {
  final String thingId;
  final String proposalId;

  GetVerifierLotteryParticipants({
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

class CastVoteOffChain extends SettlementActionAwaitable<CastVoteResultVm> {
  final String thingId;
  final String proposalId;
  final DecisionIm decision;
  final String reason;

  CastVoteOffChain({
    required this.thingId,
    required this.proposalId,
    required this.decision,
    required this.reason,
  });
}

class CastVoteOnChain extends SettlementActionAwaitable<CastVoteResultVm> {
  final String thingId;
  final String proposalId;
  final DecisionIm decision;
  final String reason;

  CastVoteOnChain({
    required this.thingId,
    required this.proposalId,
    required this.decision,
    required this.reason,
  });
}

class GetVerifiers extends SettlementAction {
  final String proposalId;

  GetVerifiers({required this.proposalId});
}
