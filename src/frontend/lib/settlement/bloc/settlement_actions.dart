import '../../general/bloc/mixins.dart';
import 'settlement_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class SettlementAction {}

abstract class SettlementActionAwaitable<T extends SettlementResultVm?>
    extends SettlementAction with AwaitableResult<T> {}

class GetSettlementProposalsFor extends SettlementAction {
  final String thingId;

  GetSettlementProposalsFor({required this.thingId});
}

class CreateNewSettlementProposalDraft extends SettlementActionAwaitable<
    CreateNewSettlementProposalDraftResultVm> {
  final DocumentContext documentContext;

  CreateNewSettlementProposalDraft({required this.documentContext});
}
