import '../models/vm/get_contracts_states_rvm.dart';
import '../models/vm/tag_vm.dart';
import 'general_api_service.dart';

class GeneralService {
  final GeneralApiService _generalApiService;

  GeneralService(this._generalApiService);

  Future<List<TagVm>> getTags() async {
    var tags = await _generalApiService.getTags();
    return tags;
  }

  Future<GetContractsStatesRvm> getContractsStates() => _generalApiService.getContractsStates();
}
