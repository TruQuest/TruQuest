import 'dart:async';

import '../../general/contexts/document_context.dart';
import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(this._thingApiService);

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
      [1],
    );

    _progress$Channel.add(progress$);
    return progress$;
  }
}
