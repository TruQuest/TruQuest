import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../../user/services/user_service.dart';
import '../../user/services/user_api_service.dart';
import '../models/vm/notification_vm.dart';
import '../models/vm/watched_item_type_vm.dart';
import 'server_connector.dart';

class NotificationsCache {
  final UserService _userService;
  final UserApiService _userApiService;

  final _userIdToUnreadNotifications = <String?, Set<NotificationVm>>{};
  Set<NotificationVm>? _unreadNotifications;
  String? _userId;

  final _unreadNotificationsCountChannel = BehaviorSubject<int>();
  Stream<int> get unreadNotificationsCount$ => _unreadNotificationsCountChannel.stream;

  final _unreadNotificationsChannel = BehaviorSubject<(List<NotificationVm>, String?)>();
  Stream<(List<NotificationVm>, String?)> get unreadNotifications$ => _unreadNotificationsChannel.stream;

  NotificationsCache(
    this._userService,
    this._userApiService,
    ServerConnector serverConnector,
  ) {
    serverConnector.serverEvent$.where((event) => event.$1 == ServerEventType.notification).listen((event) {
      var (notification, payload) = event.$2 as (NotificationEventType, Object);
      if (notification == NotificationEventType.initialRetrieve) {
        var (userId, notifications) = payload as (String, List<NotificationVm>);
        _onInitialRetrieve(userId, notifications);
      } else if (notification == NotificationEventType.newOne) {
        var (userId, updateTimestamp, itemType, itemId, itemUpdateCategory, title, details) =
            payload as (String?, int, WatchedItemTypeVm, String, int, String, String?);

        _onNewNotification(
          userId,
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
      _unreadNotifications = _userIdToUnreadNotifications.putIfAbsent(
        user.id,
        () => {},
      );
      _userId = user.id;
      _notify();
    });
  }

  void _notify() {
    _unreadNotificationsChannel.add(
      (
        List.unmodifiable(
          List<NotificationVm>.from(_unreadNotifications ?? {})
            ..sort(
              (n1, n2) => n2.updateTimestamp.compareTo(n1.updateTimestamp),
            ),
        ),
        _userId,
      ),
    );
    _unreadNotificationsCountChannel.add(_unreadNotifications?.length ?? 0);
  }

  void _onInitialRetrieve(String userId, List<NotificationVm> notifications) {
    // @@NOTE: In theory, it's highly unlikely but still possible that this runs
    // before current user is determined, so the set could be absent.
    var usersUnreadNotifications = _userIdToUnreadNotifications.putIfAbsent(
      userId,
      () => {},
    );
    usersUnreadNotifications.addAll(notifications);
    _notify();
  }

  void _onNewNotification(
    String? userId,
    int updateTimestamp,
    WatchedItemTypeVm itemType,
    String itemId,
    int itemUpdateCategory,
    String title,
    String? details,
  ) {
    var usersUnreadNotifications = _userIdToUnreadNotifications.putIfAbsent(
      userId,
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

  Future remove(List<NotificationVm> notifications, String? userId) async {
    var usersUnreadNotifications = _userIdToUnreadNotifications[userId]!;
    usersUnreadNotifications.removeAll(notifications);
    _notify();

    if (userId != null) {
      var latestNotificationsPerItemAndCategory = <String, NotificationVm>{};
      for (var notification in notifications) {
        latestNotificationsPerItemAndCategory.update(
          notification.key,
          (n) => notification.updateTimestamp.isAfter(n.updateTimestamp) ? notification : n,
          ifAbsent: () => notification,
        );
      }

      // @@NOTE: Updates from watched items could be mixed in with ephemeral notifications,
      // but that's alright, since those would simply be ignored on the server.
      await _userApiService.markNotificationsAsRead(
        latestNotificationsPerItemAndCategory.values.toList(),
      );
    }
  }
}
