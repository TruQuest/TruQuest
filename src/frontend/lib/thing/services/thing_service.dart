import 'dart:async';

import '../models/rvm/get_thing_result_vm.dart';
import '../../general/contracts/truquest_contract.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/submit_new_thing_result_vm.dart';
import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;
  final TruQuestContract _truQuestContract;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(this._thingApiService, this._truQuestContract);

  Future<Stream<int>> createNewThingDraft(
    DocumentContext documentContext,
  ) async {
    var progress$ = await _thingApiService.createNewThingDraft(
      documentContext.subjectId!,
      documentContext.nameOrTitle!,
      documentContext.details!,
      documentContext.imageExt,
      documentContext.imageBytes,
      documentContext.croppedImageBytes,
      documentContext.evidence,
      [1, 2, 3],
    );

    _progress$Channel.add(progress$);
    return progress$;
  }

  Future<GetThingResultVm> getThing(String thingId) async {
    var result = await _thingApiService.getThing(thingId);
    for (var e in result.thing.evidence) {
      print('${e.originUrl} ${e.ipfsCid} ${e.previewImageIpfsCid}');
    }
    for (var t in result.thing.tags) {
      print(t.name);
    }
    return result;
  }

  Future<SubmitNewThingResultVm> submitNewThing(String thingId) async {
    var result = await _thingApiService.submitNewThing(thingId);
    print(result.thingId);
    print(result.signature);
    return result;
  }

  Future fundThing(String thingId, String signature) async {
    await _truQuestContract.fundThing(thingId, signature);
  }
}
