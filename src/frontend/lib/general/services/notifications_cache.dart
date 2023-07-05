import 'dart:async';

import 'package:flutter/scheduler.dart';
import 'package:rxdart/rxdart.dart';
import 'package:universal_html/html.dart' as html;

import '../../ethereum/services/ethereum_service.dart';
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
  String? _username;

  final BehaviorSubject<int> _unreadNotificationsCountChannel =
      BehaviorSubject<int>();
  Stream<int> get unreadNotificationsCount$ =>
      _unreadNotificationsCountChannel.stream;

  final BehaviorSubject<(List<NotificationVm>, String?)>
      _unreadNotificationsChannel =
      BehaviorSubject<(List<NotificationVm>, String?)>();
  Stream<(List<NotificationVm>, String?)> get unreadNotifications$ =>
      _unreadNotificationsChannel.stream;

  NotificationVm? _clientSideWarning;

  NotificationsCache(
    this._userService,
    this._userApiService,
    ServerConnector serverConnector,
    EthereumService ethereumService,
  ) {
    serverConnector.serverEvent$
        .where((event) => event.$1 == ServerEventType.notification)
        .listen((event) {
      var (notification, payload) = event.$2 as (NotificationEventType, Object);
      if (notification == NotificationEventType.initialRetrieve) {
        var (username, notifications) =
            payload as (String, List<NotificationVm>);
        _onInitialRetrieve(username, notifications);
      } else if (notification == NotificationEventType.newOne) {
        var (
          username,
          updateTimestamp,
          itemType,
          itemId,
          itemUpdateCategory,
          title,
          details
        ) = payload as (
          String?,
          int,
          WatchedItemTypeVm,
          String,
          int,
          String,
          String?
        );

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

    if (!ethereumService.isAvailable) {
      _clientSideWarning = NotificationVm(
        updateTimestamp: DateTime.now(),
        itemType: WatchedItemTypeVm.subject,
        itemId: '',
        itemUpdateCategory: 0,
        title: 'Please install Metamask or Coinbase wallet extension',
        details: 'Reload the page after installation',
      );
    } else {
      if (!ethereumService.multipleWalletsDetected ||
          ethereumService.walletSelected.isCompleted) {
        _listenForChainChanges(ethereumService);
      } else {
        ethereumService.walletSelected.future.then((_) {
          _listenForChainChanges(ethereumService);
        });
      }
    }

    _userService.currentUserChanged$.listen((user) {
      _unreadNotifications = _usernameToUnreadNotifications.putIfAbsent(
        user.username,
        () => {},
      );
      _username = user.username;
      _notify();
    });
  }

  void _listenForChainChanges(EthereumService ethereumService) {
    ethereumService.connectedChainChanged$.listen((event) {
      var (chainId, shouldReloadPage) = event;
      if (shouldReloadPage) {
        SchedulerBinding.instance.addPostFrameCallback(
          (_) => html.window.location.reload(),
        );
      }

      if (chainId != ethereumService.validChainId) {
        _clientSideWarning = NotificationVm(
          updateTimestamp: DateTime.now(),
          itemType: WatchedItemTypeVm.subject,
          itemId: '',
          itemUpdateCategory: 1,
          title: 'You are on an unsupported chain',
          details: 'Click to switch',
        );
      } else {
        _clientSideWarning = null;
      }

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
            )
            ..insertAll(
              0,
              _clientSideWarning != null ? [_clientSideWarning!] : [],
            ),
        ),
        _username,
      ),
    );
    _unreadNotificationsCountChannel.add(
      _clientSideWarning != null ? -1 : _unreadNotifications?.length ?? 0,
    );
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

  Future remove(List<NotificationVm> notifications, String? username) async {
    notifications = notifications.where((n) => n.itemId != '').toList();
    var usersUnreadNotifications = _usernameToUnreadNotifications[username]!;
    usersUnreadNotifications.removeAll(notifications);
    _notify();

    if (username != null) {
      var latestNotificationsPerItemAndCategory = <String, NotificationVm>{};
      for (var notification in notifications) {
        if (!latestNotificationsPerItemAndCategory.containsKey(
          notification.key,
        )) {
          latestNotificationsPerItemAndCategory[notification.key] =
              notification;
        } else if (notification.updateTimestamp.isAfter(
          latestNotificationsPerItemAndCategory[notification.key]!
              .updateTimestamp,
        )) {
          latestNotificationsPerItemAndCategory[notification.key] =
              notification;
        }
      }

      // @@NOTE: Updates from watched items could be mixed in with ephemeral notifications,
      // but that's alright, since those would simply be ignored on the server.
      await _userApiService.markNotificationsAsRead(
        latestNotificationsPerItemAndCategory.values.toList(),
      );
    }
  }
}
