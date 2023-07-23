import 'dart:async';

import '../../user/errors/wallet_locked_error.dart';
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

  final _proposalChannel =
      StreamController<GetSettlementProposalRvm>.broadcast();
  Stream<GetSettlementProposalRvm> get proposal$ => _proposalChannel.stream;

  final _verifierLotteryInfoChannel =
      StreamController<GetVerifierLotteryInfoSuccessVm>.broadcast();
  Stream<GetVerifierLotteryInfoSuccessVm> get verifierLotteryInfo$ =>
      _verifierLotteryInfoChannel.stream;

  final _verifierLotteryParticipantsChannel =
      StreamController<GetVerifierLotteryParticipantsRvm>.broadcast();
  Stream<GetVerifierLotteryParticipantsRvm> get verifierLotteryParticipants$ =>
      _verifierLotteryParticipantsChannel.stream;

  final _verifiersChannel = StreamController<GetVerifiersRvm>.broadcast();
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
    try {
      await _settlementService.fundSettlementProposal(
        action.thingId,
        action.proposalId,
        action.signature,
      );
      action.complete(null);
    } on WalletLockedError catch (error) {
      action.complete(FundSettlementProposalFailureVm(error: error));
    }
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

  void _claimLotterySpot(ClaimLotterySpot action) async {
    try {
      await _settlementService.claimLotterySpot(
        action.thingId,
        action.proposalId,
        action.userIndexInThingVerifiersArray,
      );
      _refreshVerifierLotteryInfo(
        action.thingId,
        action.proposalId,
      );
      action.complete(null);
    } on WalletLockedError catch (error) {
      action.complete(ClaimLotterySpotFailureVm(error: error));
    }
  }

  void _joinLottery(JoinLottery action) async {
    try {
      await _settlementService.joinLottery(
        action.thingId,
        action.proposalId,
      );
      _refreshVerifierLotteryInfo(
        action.thingId,
        action.proposalId,
      );
      action.complete(null);
    } on WalletLockedError catch (error) {
      action.complete(JoinLotteryFailureVm(error: error));
    }
  }

  void _getVerifierLotteryParticipants(
    GetVerifierLotteryParticipants action,
  ) async {
    var result = await _settlementService.getVerifierLotteryParticipants(
      action.thingId,
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
        userId: info.$1,
        initBlock: info.$2,
        durationBlocks: info.$3,
        userIndexInProposalVerifiersArray: info.$4,
      ),
    );
  }

  void _castVoteOffChain(CastVoteOffChain action) async {
    try {
      await _settlementService.castVoteOffChain(
        action.thingId,
        action.proposalId,
        action.decision,
        action.reason,
      );
      action.complete(null);
    } on WalletLockedError catch (error) {
      action.complete(CastVoteOffChainFailureVm(error: error));
    }
  }

  void _castVoteOnChain(CastVoteOnChain action) async {
    try {
      await _settlementService.castVoteOnChain(
        action.thingId,
        action.proposalId,
        action.userIndexInProposalVerifiersArray,
        action.decision,
        action.reason,
      );
      action.complete(null);
    } on WalletLockedError catch (error) {
      action.complete(CastVoteOnChainFailureVm(error: error));
    }
  }

  void _getVerifiers(GetVerifiers action) async {
    var result = await _settlementService.getVerifiers(action.proposalId);
    _verifiersChannel.add(result);
  }
}
