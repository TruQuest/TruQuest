import 'dart:async';

import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../models/rvm/get_settlement_proposals_rvm.dart';
import '../../general/bloc/bloc.dart';
import 'settlement_actions.dart';
import 'settlement_result_vm.dart';
import '../services/settlement_service.dart';

class SettlementBloc extends Bloc<SettlementAction> {
  final SettlementService _settlementService;

  final StreamController<GetSettlementProposalsRvm> _proposalsChannel =
      StreamController<GetSettlementProposalsRvm>.broadcast();
  Stream<GetSettlementProposalsRvm> get proposals$ => _proposalsChannel.stream;

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
      if (action is GetSettlementProposalsFor) {
        _getSettlementProposalsFor(action);
      } else if (action is CreateNewSettlementProposalDraft) {
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
      } else if (action is UnsubscribeFromProposal) {
        _unsubscribeFromProposal(action);
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

  @override
  void dispose({SettlementAction? cleanupAction}) {
    // TODO: implement dispose
  }

  void _getSettlementProposalsFor(GetSettlementProposalsFor action) async {
    var result = await _settlementService.getSettlementProposalsFor(
      action.thingId,
    );
    _proposalsChannel.add(result);
  }

  void _createNewSettlementProposalDraft(
    CreateNewSettlementProposalDraft action,
  ) async {
    await _settlementService.createNewSettlementProposalDraft(
      action.documentContext,
    );
    action.complete(CreateNewSettlementProposalDraftSuccessVm());
  }

  void _getSettlementProposal(GetSettlementProposal action) async {
    var result = await _settlementService.getSettlementProposal(
      action.proposalId,
    );
    if (action.subscribe) {
      await _settlementService.subscribeToProposal(action.proposalId);
    }

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

  void _unsubscribeFromProposal(UnsubscribeFromProposal action) async {
    await _settlementService.unsubscribeFromProposal(action.proposalId);
  }

  void _submitNewSettlementProposal(SubmitNewSettlementProposal action) async {
    await _settlementService.submitNewSettlementProposal(action.proposalId);
    action.complete(SubmitNewSettlementProposalSuccessVm());
  }

  void _fundSettlementProposal(FundSettlementProposal action) async {
    await _settlementService.fundSettlementProposal(
      action.thingId,
      action.proposalId,
      action.signature,
    );
    action.complete(FundSettlementProposalSuccessVm());
  }

  void _getVerifierLotteryInfo(GetVerifierLotteryInfo action) async {
    var info = await _settlementService.getVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }

  void _claimLotterySpot(ClaimLotterySpot action) async {
    await _settlementService.claimLotterySpot(
      action.thingId,
      action.proposalId,
    );

    var info = await _settlementService.getVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }

  void _preJoinLottery(PreJoinLottery action) async {
    await _settlementService.preJoinLottery(action.thingId, action.proposalId);

    var info = await _settlementService.getVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }

  void _joinLottery(JoinLottery action) async {
    await _settlementService.joinLottery(action.thingId, action.proposalId);

    var info = await _settlementService.getVerifierLotteryInfo(
      action.thingId,
      action.proposalId,
    );
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
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
        initBlock: info.item1,
        durationBlocks: info.item2,
        isDesignatedVerifier: info.item3,
        latestBlockNumber: info.item4,
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
  }

  void _castVoteOnChain(CastVoteOnChain action) async {
    await _settlementService.castVoteOnChain(
      action.thingId,
      action.proposalId,
      action.decision,
      action.reason,
    );
  }

  void _getVerifiers(GetVerifiers action) async {
    var result = await _settlementService.getVerifiers(action.proposalId);
    _verifiersChannel.add(result);
  }
}
