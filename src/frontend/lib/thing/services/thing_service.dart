import 'dart:async';

import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(this._thingApiService);

  Future<Stream<int>> createNewThingDraft() async {
    var progress$ = await _thingApiService.createNewThingDraft();
    _progress$Channel.add(progress$);
    return progress$;
  }
}
