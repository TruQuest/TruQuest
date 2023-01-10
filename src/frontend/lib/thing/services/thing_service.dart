import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;

  ThingService(this._thingApiService);

  Future<Stream<int>> createNewThingDraft() async {
    var progress$ = await _thingApiService.createNewThingDraft();
    return progress$;
  }
}
