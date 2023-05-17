import '../models/rvm/tag_vm.dart';
import 'general_api_service.dart';

class GeneralService {
  final GeneralApiService _generalApiService;

  GeneralService(this._generalApiService);

  Future<List<TagVm>> getTags() async {
    var tags = await _generalApiService.getTags();
    return tags;
  }
}
