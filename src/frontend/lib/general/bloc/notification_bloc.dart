import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../../thing/services/thing_service.dart';
import '../../thing/services/thing_api_service.dart';
import 'bloc.dart';

class NotificationBloc extends Bloc {
  final ThingService _thingService;
  final ThingApiService _thingApiService;

  final StreamController<String> _notificationChannel =
      StreamController<String>.broadcast();
  Stream<String> get notification$ => _notificationChannel.stream;

  final BehaviorSubject<Stream<int>?> _progress$Channel =
      BehaviorSubject<Stream<int>?>();
  Stream<Stream<int>?> get progress$$ => _progress$Channel.stream;

  NotificationBloc(this._thingService, this._thingApiService) {
    _thingApiService.thingEvent$.listen((event) {
      _notificationChannel.add(event);
    });

    _thingService.progress$$.listen((progress$) {
      _progress$Channel.add(progress$);
      progress$.listen(null, onDone: () {
        Future.delayed(Duration(seconds: 2)).then(
          (_) {
            if (_progress$Channel.value == progress$) {
              _progress$Channel.add(null);
            }
          },
        );
      });
    });
  }

  @override
  void dispose({cleanupAction}) {
    // TODO: implement dispose
  }
}
