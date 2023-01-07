import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;

  ThingService(this._thingApiService);

  Future createNewThingDraft() async {
    await _thingApiService.createNewThingDraft();
  }
}
