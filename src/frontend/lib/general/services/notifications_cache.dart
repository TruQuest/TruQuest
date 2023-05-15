import 'dart:async';

import 'package:rxdart/rxdart.dart';
import 'package:tuple/tuple.dart';

import '../../user/services/user_service.dart';
import '../../user/services/user_api_service.dart';
import '../models/rvm/notification_vm.dart';
import '../models/rvm/watched_item_type_vm.dart';
import 'server_connector.dart';

class NotificationsCache {
  final UserService _userService;
  final UserApiService _userApiService;

  final Map<String?, Set<NotificationVm>> _usernameToUnreadNotifications = {};
  Set<NotificationVm>? _unreadNotifications;

  final BehaviorSubject<int> _unreadNotificationsCountChannel =
      BehaviorSubject<int>();
  Stream<int> get unreadNotificationsCount$ =>
      _unreadNotificationsCountChannel.stream;

  final BehaviorSubject<List<NotificationVm>> _unreadNotificationsChannel =
      BehaviorSubject<List<NotificationVm>>();
  Stream<List<NotificationVm>> get unreadNotifications$ =>
      _unreadNotificationsChannel.stream;

  NotificationsCache(
    this._userService,
    this._userApiService,
    ServerConnector serverConnector,
  ) {
    serverConnector.serverEvent$
        .where((event) => event.item1 == ServerEventType.notification)
        .listen((event) {
      var notification = event.item2 as Tuple2<NotificationEventType, Object>;
      if (notification.item1 == NotificationEventType.initialRetrieve) {
        var payload =
            notification.item2 as Tuple2<String, List<NotificationVm>>;
        _onInitialRetrieve(payload.item1, payload.item2);
      } else if (notification.item1 == NotificationEventType.newOne) {
        var payload = notification.item2 as Tuple7<String?, int,
            WatchedItemTypeVm, String, int, String, String?>;
        var username = payload.item1;
        var updateTimestamp = payload.item2;
        var itemType = payload.item3;
        var itemId = payload.item4;
        var itemUpdateCategory = payload.item5;
        var title = payload.item6;
        var details = payload.item7;

        _onNewNotification(
          username,
          updateTimestamp,
          itemType,
          itemId,
          itemUpdateCategory,
          title,
          details,
        );
      }
    });

    _userService.currentUserChanged$.listen((user) {
      _unreadNotifications = _usernameToUnreadNotifications.putIfAbsent(
        user.username,
        () => {},
      );
      _notify();
    });
  }

  void _notify() {
    _unreadNotificationsChannel.add(
      List.unmodifiable(
        List.from(_unreadNotifications ?? {})
          ..sort((n1, n2) => n2.updateTimestamp.compareTo(n1.updateTimestamp)),
      ),
    );
    _unreadNotificationsCountChannel.add(_unreadNotifications?.length ?? 0);
  }

  void _onInitialRetrieve(
    String username,
    List<NotificationVm> notifications,
  ) {
    // @@NOTE: In theory, it's highly unlikely but still possible that this runs
    // before current user is determined, so the set could be absent.
    var usersUnreadNotifications = _usernameToUnreadNotifications.putIfAbsent(
      username,
      () => {},
    );
    usersUnreadNotifications.addAll(notifications);
    _notify();
  }

  void _onNewNotification(
    String? username,
    int updateTimestamp,
    WatchedItemTypeVm itemType,
    String itemId,
    int itemUpdateCategory,
    String title,
    String? details,
  ) {
    var usersUnreadNotifications = _usernameToUnreadNotifications.putIfAbsent(
      username,
      () => {},
    );
    usersUnreadNotifications.add(NotificationVm(
      updateTimestamp: DateTime.fromMillisecondsSinceEpoch(updateTimestamp),
      itemType: itemType,
      itemId: itemId,
      itemUpdateCategory: itemUpdateCategory,
      title: title,
      details: details,
    ));
    _notify();
  }

  Future remove(List<NotificationVm> notifications) async {
    _unreadNotifications!.removeAll(notifications);
    _notify();
    await _userApiService.markNotificationsAsRead(notifications);
  }
}
