import '../models/vm/tag_vm.dart';
import 'bloc.dart';
import 'general_actions.dart';
import '../services/general_service.dart';

class GeneralBloc extends Bloc<GeneralAction> {
  final GeneralService _generalService;

  GeneralBloc(super.toastMessenger, this._generalService);

  @override
  Future<Object?> handleExecute(GeneralAction action) {
    if (action is GetTags) {
      return _getTags(action);
    }

    throw UnimplementedError();
  }

  Future<List<TagVm>> _getTags(GetTags action) => _generalService.getTags();
}
