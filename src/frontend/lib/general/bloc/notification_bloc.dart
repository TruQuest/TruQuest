import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../models/vm/notification_vm.dart';
import '../services/notifications_cache.dart';
import 'notification_actions.dart';
import '../../settlement/services/settlement_service.dart';
import '../../thing/services/thing_service.dart';
import 'bloc.dart';

class NotificationBloc extends Bloc<NotificationAction> {
  final NotificationsCache _notificationsCache;
  final ThingService _thingService;
  final SettlementService _settlementService;

  final _progress$Channel = BehaviorSubject<Stream<int>?>();
  Stream<Stream<int>?> get progress$$ => _progress$Channel.stream;

  final _toastChannel = StreamController<String>.broadcast();
  Stream<String> get toast$ => _toastChannel.stream;

  Stream<(List<NotificationVm>, String?)> get unreadNotifications$ => _notificationsCache.unreadNotifications$;

  NotificationBloc(
    this._notificationsCache,
    super.toastMessenger,
    this._thingService,
    this._settlementService,
  ) {
    onAction = (action) async {
      if (action is Dismiss) {
        await _dismiss(action);
      }
    };

    _thingService.progress$$.listen((progress$) {
      _progress$Channel.add(progress$);
      progress$.listen(null, onDone: () {
        Future.delayed(const Duration(seconds: 2)).then(
          (_) {
            if (_progress$Channel.value == progress$) {
              _progress$Channel.add(null);
            }
          },
        );
      });
    });

    _settlementService.progress$$.listen((progress$) {
      _progress$Channel.add(progress$);
      progress$.listen(null, onDone: () {
        Future.delayed(const Duration(seconds: 2)).then(
          (_) {
            if (_progress$Channel.value == progress$) {
              _progress$Channel.add(null);
            }
          },
        );
      });
    });

    toastMessenger.toast$.listen((toast) => _toastChannel.add(toast));
  }

  Future _dismiss(Dismiss action) async {
    await _notificationsCache.remove(action.notifications, action.userId);
  }
}
