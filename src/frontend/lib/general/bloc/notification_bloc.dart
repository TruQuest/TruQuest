import 'dart:async';

import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';

import '../services/toast_messenger.dart';
import '../services/notifications_cache.dart';
import 'notification_actions.dart';
import '../../settlement/services/settlement_service.dart';
import '../../thing/services/thing_service.dart';
import 'bloc.dart';

class NotificationBloc extends Bloc<NotificationAction> {
  final NotificationsCache _notificationsCache;
  final ToastMessenger _toastMessenger;
  final ThingService _thingService;
  // final SettlementService _settlementService;

  final BehaviorSubject<Stream<int>?> _progress$Channel =
      BehaviorSubject<Stream<int>?>();
  Stream<Stream<int>?> get progress$$ => _progress$Channel.stream;

  final StreamController<Widget> _toastChannel =
      StreamController<Widget>.broadcast();
  Stream<Widget> get toast$ => _toastChannel.stream;

  NotificationBloc(
    this._notificationsCache,
    this._toastMessenger,
    this._thingService,
    // this._settlementService,
  ) {
    actionChannel.stream.listen((action) {
      if (action is Dismiss) {
        _dismiss(action);
      }
    });

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

    // _settlementService.progress$$.listen((progress$) {
    //   _progress$Channel.add(progress$);
    //   progress$.listen(null, onDone: () {
    //     Future.delayed(const Duration(seconds: 2)).then(
    //       (_) {
    //         if (_progress$Channel.value == progress$) {
    //           _progress$Channel.add(null);
    //         }
    //       },
    //     );
    //   });
    // });

    _toastMessenger.toast$.listen((toast) => _toastChannel.add(toast));
  }

  void _dismiss(Dismiss action) async {
    await _notificationsCache.remove(action.notifications, action.username);
  }
}
