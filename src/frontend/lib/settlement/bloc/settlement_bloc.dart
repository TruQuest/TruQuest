import 'dart:async';

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

  SettlementBloc(this._settlementService) {
    actionChannel.stream.listen((action) {
      if (action is GetSettlementProposalsFor) {
        _getSettlementProposalsFor(action);
      } else if (action is CreateNewSettlementProposalDraft) {
        _createNewSettlementProposalDraft(action);
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
}
