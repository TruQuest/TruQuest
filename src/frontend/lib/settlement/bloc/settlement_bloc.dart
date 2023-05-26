import 'dart:async';

import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../../general/bloc/bloc.dart';
import 'settlement_actions.dart';
import 'settlement_result_vm.dart';
import '../services/settlement_service.dart';

class SettlementBloc extends Bloc<SettlementAction> {
  final SettlementService _settlementService;

  final StreamController<GetSettlementProposalRvm> _proposalChannel =
      StreamController<GetSettlementProposalRvm>.broadcast();
  Stream<GetSettlementProposalRvm> get proposal$ => _proposalChannel.stream;

  final StreamController<GetVerifierLotteryInfoSuccessVm>
      _verifierLotteryInfoChannel =
      StreamController<GetVerifierLotteryInfoSuccessVm>.broadcast();
  Stream<GetVerifierLotteryInfoSuccessVm> get verifierLotteryInfo$ =>
      _verifierLotteryInfoChannel.stream;

  final StreamController<GetVerifierLotteryParticipantsRvm>
      _verifierLotteryParticipantsChannel =
      StreamController<GetVerifierLotteryParticipantsRvm>.broadcast();
  Stream<GetVerifierLotteryParticipantsRvm> get verifierLotteryParticipants$ =>
      _verifierLotteryParticipantsChannel.stream;

  final StreamController<GetVerifiersRvm> _verifiersChannel =
      StreamController<GetVerifiersRvm>.broadcast();
  Stream<GetVerifiersRvm> get verifiers$ => _verifiersChannel.stream;

  SettlementBloc(this._settlementService) {
    actionChannel.stream.listen((action) {
      if (action is CreateNewSettlementProposalDraft) {
        _createNewSettlementProposalDraft(action);
      } else if (action is GetSettlementProposal) {
        _getSettlementProposal(action);
      } else if (action is SubmitNewSettlementProposal) {
        _submitNewSettlementProposal(action);
      } else if (action is FundSettlementProposal) {
        _fundSettlementProposal(action);
      } else if (action is GetVerifierLotteryInfo) {
        _getVerifierLotteryInfo(action);
      } else if (action is ClaimLotterySpot) {
        _claimLotterySpot(action);
      } else if (action is PreJoinLottery) {
        _preJoinLottery(action);
      } else if (action is JoinLottery) {
        _joinLottery(action);
      } else if (action is GetVerifierLotteryParticipants) {
        _getVerifierLotteryParticipants(action);
      } else if (action is GetAssessmentPollInfo) {
        _getAssessmentPollInfo(action);
      } else if (action is CastVoteOffChain) {
        _castVoteOffChain(action);
      } else if (action is CastVoteOnChain) {
        _castVoteOnChain(action);
      } else if (action is GetVerifiers) {
        _getVerifiers(action);
      }
    });
  }

  void _createNewSettlementProposalDraft(
    CreateNewSettlementProposalDraft action,
  ) async {
    await _settlementService.createNewSettlementProposalDraft(
      action.documentContext,
    );
    action.complete(null);
  }

  void _getSettlementProposal(GetSettlementProposal action) async {
    var result = await _settlementService.getSettlementProposal(
      action.proposalId,
    );
    if (result.proposal.state == SettlementProposalStateVm.awaitingFunding) {
      bool aProposalAlreadyBeingAssessed = await _settlementService
          .checkThingAlreadyHasSettlementProposalUnderAssessment(
        result.proposal.thingId,
      );
      result = GetSettlementProposalRvm(
        proposal: result.proposal.copyWith(
          canBeFunded: !aProposalAlreadyBeingAssessed,
        ),
        signature: result.signature,
      );
    }

    _proposalChannel.add(result);
  }

  void _submitNewSettlementProposal(SubmitNewSettlementProposal action) async {
    await _settlementService.submitNewSettlementProposal(action.proposalId);
    action.complete(null);
  }

  void _fundSettlementProposal(FundSettlementProposal action) async {
    await _settlementService.fundSettlementProposal(
      action.thingId,
      action.proposalId,
      action.signature,
    );
    action.complete(null);
  }

  void _refreshVerifierLotteryInfo(
    String thingId,
    String proposalId,
  ) async {
    var info = await _settlementService.getVerifierLotteryInfo(
      thingId,
      proposalId,
    );
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        initBlock: info.$1,
        durationBlocks: info.$2,
        alreadyClaimedASpot: info.$3,
        alreadyPreJoined: info.$4,
        alreadyJoined: info.$5,
        latestBlockNumber: info.$6,
      ),
    );
  }

  void _getVerifierLotteryInfo(GetVerifierLotteryInfo action) {
    _refreshVerifierLotteryInfo(action.thingId, action.proposalId);
  }

  void _claimLotterySpot(ClaimLotterySpot action) async {
    var error = await _settlementService.claimLotterySpot(
      action.thingId,
      action.proposalId,
    );
    action.complete(error != null ? ClaimLotterySpotFailureVm() : null);
    _refreshVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
  }

  void _preJoinLottery(PreJoinLottery action) async {
    var error = await _settlementService.preJoinLottery(
      action.thingId,
      action.proposalId,
    );
    action.complete(error != null ? PreJoinLotteryFailureVm() : null);
    _refreshVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
  }

  void _joinLottery(JoinLottery action) async {
    var error = await _settlementService.joinLottery(
      action.thingId,
      action.proposalId,
    );
    action.complete(error != null ? JoinLotteryFailureVm() : null);
    _refreshVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
  }

  void _getVerifierLotteryParticipants(
    GetVerifierLotteryParticipants action,
  ) async {
    var result = await _settlementService.getVerifierLotteryParticipants(
      action.proposalId,
    );
    _verifierLotteryParticipantsChannel.add(result);
  }

  void _getAssessmentPollInfo(GetAssessmentPollInfo action) async {
    var info = await _settlementService.getAssessmentPollInfo(
      action.thingId,
      action.proposalId,
    );
    action.complete(
      GetAssessmentPollInfoSuccessVm(
        initBlock: info.$1,
        durationBlocks: info.$2,
        isDesignatedVerifier: info.$3,
        latestBlockNumber: info.$4,
      ),
    );
  }

  void _castVoteOffChain(CastVoteOffChain action) async {
    await _settlementService.castVoteOffChain(
      action.thingId,
      action.proposalId,
      action.decision,
      action.reason,
    );
    action.complete(CastVoteResultVm());
  }

  void _castVoteOnChain(CastVoteOnChain action) async {
    await _settlementService.castVoteOnChain(
      action.thingId,
      action.proposalId,
      action.decision,
      action.reason,
    );
    action.complete(CastVoteResultVm());
  }

  void _getVerifiers(GetVerifiers action) async {
    var result = await _settlementService.getVerifiers(action.proposalId);
    _verifiersChannel.add(result);
  }
}
