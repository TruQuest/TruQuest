import 'dart:async';

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
      // await _settlementService.subscribeToProposal(action.proposalId);
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

  void _submitNewSettlementProposal(SubmitNewSettlementProposal action) async {
    var result = await _settlementService.submitNewSettlementProposal(
      action.proposalId,
    );
    action.complete(
      SubmitNewSettlementProposalSuccessVm(signature: result.signature),
    );
  }
}
