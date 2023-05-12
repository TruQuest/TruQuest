import 'dart:async';

import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';

import '../services/notifications_cache.dart';
import 'notification_actions.dart';
import '../../settlement/models/rvm/settlement_proposal_state_vm.dart';
import '../../settlement/services/settlement_api_service.dart';
import '../../settlement/services/settlement_service.dart';
import '../../thing/models/rvm/thing_state_vm.dart';
import '../services/server_connector.dart';
import '../../thing/services/thing_service.dart';
import '../../thing/services/thing_api_service.dart';
import '../contexts/page_context.dart';
import 'bloc.dart';

class NotificationBloc extends Bloc<NotificationAction> {
  final NotificationsCache _notificationsCache;
  final ThingService _thingService;
  final ThingApiService _thingApiService;
  final SettlementService _settlementService;
  final SettlementApiService _settlementApiService;

  final StreamController<Widget Function(PageContext)> _notificationChannel =
      StreamController<Widget Function(PageContext)>.broadcast();
  Stream<Widget Function(PageContext)> get notification$ =>
      _notificationChannel.stream;

  final BehaviorSubject<Stream<int>?> _progress$Channel =
      BehaviorSubject<Stream<int>?>();
  Stream<Stream<int>?> get progress$$ => _progress$Channel.stream;

  NotificationBloc(
    this._notificationsCache,
    this._thingService,
    this._thingApiService,
    this._settlementService,
    this._settlementApiService,
  ) {
    actionChannel.stream.listen((action) {
      if (action is Dismiss) {
        _dismiss(action);
      }
    });

    _thingApiService.thingEvent$.listen((event) {
      ThingEventType eventType = event.item1;
      String thingId = event.item2;
      Object? data = event.item3;

      Row toastContent;
      if (eventType == ThingEventType.draftCreated) {
        toastContent = Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.check),
            SizedBox(width: 12),
            Text('Draft created!'),
          ],
        );
      } else {
        var state = data as ThingStateVm;
        toastContent = Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.new_releases_outlined),
            SizedBox(width: 12),
            Text('Thing updated: ${state.getString()}'),
          ],
        );
      }

      Widget toastBuilder(PageContext pageContext) => InkWell(
            onTap: () {
              pageContext.route = '/things/$thingId';
              pageContext.controller.jumpToPage(
                DateTime.now().millisecondsSinceEpoch,
              );
            },
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(25),
                color: Colors.greenAccent,
              ),
              child: toastContent,
            ),
          );

      _notificationChannel.add(toastBuilder);
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

    _settlementApiService.proposalEvent$.listen((event) {
      SettlementEventType eventType = event.item1;
      String proposalId = event.item2;
      Object? data = event.item3;

      Row toastContent;
      if (eventType == SettlementEventType.draftCreated) {
        toastContent = Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.check),
            SizedBox(width: 12),
            Text('Draft created!'),
          ],
        );
      } else {
        var state = data as SettlementProposalStateVm;
        toastContent = Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.new_releases_outlined),
            SizedBox(width: 12),
            Text('Proposal updated: ${state.getString()}'),
          ],
        );
      }

      Widget toastBuilder(PageContext pageContext) => InkWell(
            onTap: () {
              pageContext.route = '/proposals/$proposalId';
              pageContext.controller.jumpToPage(
                DateTime.now().millisecondsSinceEpoch,
              );
            },
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(25),
                color: Colors.greenAccent,
              ),
              child: toastContent,
            ),
          );

      _notificationChannel.add(toastBuilder);
    });

    _settlementService.progress$$.listen((progress$) {
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

  void _dismiss(Dismiss action) async {
    await _notificationsCache.remove(action.notifications);
  }
}
