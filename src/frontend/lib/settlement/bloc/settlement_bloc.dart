import 'dart:async';

import '../models/rvm/assessment_poll_info_vm.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../../general/models/rvm/verifier_vm.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../../general/bloc/bloc.dart';
import '../models/rvm/verifier_lottery_info_vm.dart';
import 'settlement_actions.dart';
import '../services/settlement_service.dart';

class SettlementBloc extends Bloc<SettlementAction> {
  final SettlementService _settlementService;

  final _proposalChannel = StreamController<GetSettlementProposalRvm>.broadcast();
  Stream<GetSettlementProposalRvm> get proposal$ => _proposalChannel.stream;

  final _verifierLotteryInfoChannel = StreamController<VerifierLotteryInfoVm>.broadcast();
  Stream<VerifierLotteryInfoVm> get verifierLotteryInfo$ => _verifierLotteryInfoChannel.stream;

  final _verifierLotteryParticipantsChannel = StreamController<List<VerifierLotteryParticipantEntryVm>>.broadcast();
  Stream<List<VerifierLotteryParticipantEntryVm>> get verifierLotteryParticipants$ =>
      _verifierLotteryParticipantsChannel.stream;

  final _verifiersChannel = StreamController<List<VerifierVm>>.broadcast();
  Stream<List<VerifierVm>> get verifiers$ => _verifiersChannel.stream;

  SettlementBloc(super.toastMessenger, this._settlementService) {
    actionChannel.stream.listen((action) {
      if (action is GetSettlementProposal) {
        _getSettlementProposal(action);
      } else if (action is GetVerifierLotteryInfo) {
        _getVerifierLotteryInfo(action);
      } else if (action is GetVerifierLotteryParticipants) {
        _getVerifierLotteryParticipants(action);
      } else if (action is GetVerifiers) {
        _getVerifiers(action);
      }
    });
  }

  @override
  Future<Object?> handleExecute(SettlementAction action) {
    if (action is CreateNewSettlementProposalDraft) {
      return _createNewSettlementProposalDraft(action);
    } else if (action is SubmitNewSettlementProposal) {
      _submitNewSettlementProposal(action);
    } else if (action is GetAssessmentPollInfo) {
      return _getAssessmentPollInfo(action);
    }

    throw UnimplementedError();
  }

  @override
  Stream<Object> handleMultiStageExecute(
    SettlementAction action,
    MultiStageOperationContext ctx,
  ) {
    if (action is FundSettlementProposal) {
      return _fundSettlementProposal(action, ctx);
    } else if (action is ClaimLotterySpot) {
      return _claimLotterySpot(action, ctx);
    } else if (action is JoinLottery) {
      return _joinLottery(action, ctx);
    } else if (action is CastVoteOffChain) {
      return _castVoteOffChain(action, ctx);
    } else if (action is CastVoteOnChain) {
      return _castVoteOnChain(action, ctx);
    }

    throw UnimplementedError();
  }

  Future<bool> _createNewSettlementProposalDraft(
    CreateNewSettlementProposalDraft action,
  ) async {
    await _settlementService.createNewSettlementProposalDraft(
      action.documentContext,
    );
    return true;
  }

  void _getSettlementProposal(GetSettlementProposal action) async {
    var result = await _settlementService.getSettlementProposal(
      action.proposalId,
    );
    if (result.proposal.state == SettlementProposalStateVm.awaitingFunding) {
      bool aProposalAlreadyBeingAssessed =
          await _settlementService.checkThingAlreadyHasSettlementProposalUnderAssessment(result.proposal.thingId);
      result = GetSettlementProposalRvm(
        proposal: result.proposal.copyWith(
          canBeFunded: !aProposalAlreadyBeingAssessed,
        ),
        signature: result.signature,
      );
    }

    _proposalChannel.add(result);
  }

  Future<bool> _submitNewSettlementProposal(SubmitNewSettlementProposal action) async {
    await _settlementService.submitNewSettlementProposal(action.proposalId);
    _getSettlementProposal(GetSettlementProposal(proposalId: action.proposalId));
    return true;
  }

  Stream<Object> _fundSettlementProposal(
    FundSettlementProposal action,
    MultiStageOperationContext ctx,
  ) =>
      _settlementService.fundSettlementProposal(
        action.thingId,
        action.proposalId,
        action.signature,
        ctx,
      );

  void _refreshVerifierLotteryInfo(
    String thingId,
    String proposalId,
  ) async {
    var info = await _settlementService.getVerifierLotteryInfo(
      thingId,
      proposalId,
    );
    _verifierLotteryInfoChannel.add(
      VerifierLotteryInfoVm(
        userId: info.$1,
        initBlock: info.$2,
        durationBlocks: info.$3,
        userIndexInThingVerifiersArray: info.$4,
        alreadyClaimedASpot: info.$5,
        alreadyJoined: info.$6,
      ),
    );
  }

  void _getVerifierLotteryInfo(GetVerifierLotteryInfo action) {
    _refreshVerifierLotteryInfo(action.thingId, action.proposalId);
  }

  Stream<Object> _claimLotterySpot(ClaimLotterySpot action, MultiStageOperationContext ctx) =>
      _settlementService.claimLotterySpot(
        action.thingId,
        action.proposalId,
        action.userIndexInThingVerifiersArray,
        ctx,
      );

  Stream<Object> _joinLottery(JoinLottery action, MultiStageOperationContext ctx) => _settlementService.joinLottery(
        action.thingId,
        action.proposalId,
        ctx,
      );

  void _getVerifierLotteryParticipants(
    GetVerifierLotteryParticipants action,
  ) async {
    var result = await _settlementService.getVerifierLotteryParticipants(
      action.thingId,
      action.proposalId,
    );
    _verifierLotteryParticipantsChannel.add(result.entries);
  }

  Future<AssessmentPollInfoVm> _getAssessmentPollInfo(GetAssessmentPollInfo action) async {
    var info = await _settlementService.getAssessmentPollInfo(
      action.thingId,
      action.proposalId,
    );

    return AssessmentPollInfoVm(
      userId: info.$1,
      initBlock: info.$2,
      durationBlocks: info.$3,
      userIndexInProposalVerifiersArray: info.$4,
    );
  }

  Stream<Object> _castVoteOffChain(CastVoteOffChain action, MultiStageOperationContext ctx) =>
      _settlementService.castVoteOffChain(
        action.thingId,
        action.proposalId,
        action.decision,
        action.reason,
        ctx,
      );

  Stream<Object> _castVoteOnChain(CastVoteOnChain action, MultiStageOperationContext ctx) =>
      _settlementService.castVoteOnChain(
        action.thingId,
        action.proposalId,
        action.userIndexInProposalVerifiersArray,
        action.decision,
        action.reason,
        ctx,
      );

  void _getVerifiers(GetVerifiers action) async {
    var result = await _settlementService.getVerifiers(action.proposalId);
    _verifiersChannel.add(result.verifiers);
  }
}
