import 'dart:async';

import 'package:tuple/tuple.dart';

import '../models/rvm/notification_vm.dart';
import '../models/rvm/watched_item_type_vm.dart';
import 'server_connector.dart';

class NotificationsCache {
  final List<NotificationVm> _unreadNotifications = [];

  final StreamController<int> _unreadNotificationsCountChannel =
      StreamController<int>.broadcast();
  Stream<int> get unreadNotificationsCount$ =>
      _unreadNotificationsCountChannel.stream;

  final StreamController<List<NotificationVm>> _unreadNotificationsChannel =
      StreamController<List<NotificationVm>>.broadcast();
  Stream<List<NotificationVm>> get unreadNotifications$ =>
      _unreadNotificationsChannel.stream;

  NotificationsCache(ServerConnector serverConnector) {
    serverConnector.serverEvent$
        .where((event) => event.item1 == ServerEventType.notification)
        .listen((event) {
      var notification = event.item2 as Tuple2<NotificationEventType, Object>;
      if (notification.item1 == NotificationEventType.initialRetrieve) {
        var notifications = notification.item2 as List<NotificationVm>;
        _onInitialRetrieve(notifications);
      } else if (notification.item1 == NotificationEventType.newOne) {
        var payload = notification.item2
            as Tuple5<int, WatchedItemTypeVm, String, String, String?>;
        var updateTimestamp = payload.item1;
        var itemType = payload.item2;
        var itemId = payload.item3;
        var title = payload.item4;
        var details = payload.item5;
        _onNewNotification(updateTimestamp, itemType, itemId, title, details);
      }
    });
  }

  void _notify() {
    _unreadNotificationsChannel.add(List.unmodifiable(_unreadNotifications));
    _unreadNotificationsCountChannel.add(_unreadNotifications.length);
  }

  void _onInitialRetrieve(List<NotificationVm> notifications) {
    _unreadNotifications.addAll(notifications);
    _notify();
  }

  void _onNewNotification(
    int updateTimestamp,
    WatchedItemTypeVm itemType,
    String itemId,
    String title,
    String? details,
  ) {
    _unreadNotifications.add(NotificationVm(
      updateTimestamp: DateTime.fromMillisecondsSinceEpoch(updateTimestamp),
      itemType: itemType,
      itemId: itemId,
      title: title,
      details: details,
    ));
    _notify();
  }
}
