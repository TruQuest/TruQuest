import 'dart:async';

import '../../thing/services/thing_api_service.dart';
import 'bloc.dart';

class NotificationBloc extends Bloc {
  final ThingApiService _thingApiService;

  final StreamController<String> _notificationChannel =
      StreamController<String>.broadcast();
  Stream<String> get notification$ => _notificationChannel.stream;

  NotificationBloc(this._thingApiService) {
    _thingApiService.thingEvent$.listen((event) {
      _notificationChannel.add(event);
    });
  }

  @override
  void dispose({cleanupAction}) {
    // TODO: implement dispose
  }
}
