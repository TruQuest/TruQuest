import 'dart:async';

import '../models/rvm/get_settlement_proposals_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/submit_new_settlement_proposal_rvm.dart';
import 'settlement_api_service.dart';

class SettlementService {
  final SettlementApiService _settlementApiService;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  SettlementService(this._settlementApiService);

  Future<GetSettlementProposalsRvm> getSettlementProposalsFor(
    String thingId,
  ) async {
    var result = await _settlementApiService.getSettlementProposalsFor(thingId);
    return result;
  }

  Future createNewSettlementProposalDraft(
    DocumentContext documentContext,
  ) async {
    var progress$ =
        await _settlementApiService.createNewSettlementProposalDraft(
      documentContext.thingId!,
      documentContext.nameOrTitle!,
      documentContext.verdict!,
      documentContext.details!,
      documentContext.imageExt,
      documentContext.imageBytes,
      documentContext.croppedImageBytes,
      documentContext.evidence,
    );

    _progress$Channel.add(progress$);
  }

  Future<SubmitNewSettlementProposalRvm> submitNewSettlementProposal(
    String proposalId,
  ) async {
    var result = await _settlementApiService.submitNewSettlementProposal(
      proposalId,
    );
    print(result.thingId);
    print(result.proposalId);
    print(result.signature);
    return result;
  }
}
