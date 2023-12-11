import 'dart:async';

import '../contexts/page_context.dart';
import '../errors/api_error.dart';
import '../errors/unhandled_error.dart';
import '../models/im/unsub_then_sub_to_updates_command.dart';
import '../models/im/unsubscribe_from_updates_command.dart';
import '../utils/logger.dart';
import 'server_connector.dart';
import '../errors/error.dart';
import '../models/im/subscribe_to_updates_command.dart';

class SubscriptionManager {
  final PageContext _pageContext;
  final ServerConnector _serverConnector;

  final Completer _currentRouteSet = Completer();
  String? _currentRoute;

  SubscriptionManager(this._pageContext, this._serverConnector);

  void init() {
    _pageContext.route$.listen((route) {
      if (_isSimpleRoute(route)) {
        _handleSimpleRoute(route);
      } else {
        _handleComplexRoute(route);
      }
    });

    _serverConnector.connectionTask$.listen((connectionTask) async {
      await connectionTask;
      _resubToLastRouteIfNecessary();
    });
  }

  void _resubToLastRouteIfNecessary() async {
    await _currentRouteSet.future;
    // @@NOTE: It's possible that ServerConnector connects to the hub and reaches here before
    // SubscriptionManager handles the initial route, which, in case of a complex one, would
    // lead to subscribing twice – in _handleComplexRoute and here. But it's fine, since on SignalR side
    // adding to a group is idempotent.
    if (!_isSimpleRoute(_currentRoute!)) {
      await _subscribeToUpdates(_currentRoute!);
    }
  }

  Error _wrapHubException(Exception ex) {
    var errorMessage = ex.toString();
    if (errorMessage.contains('[Unhandled]')) {
      // [<TRACE_ID>] [Unhandled] <MESSAGE>
      var errorMessageSplit = errorMessage.split(' [Unhandled] ');
      return UnhandledError(
        errorMessageSplit.last,
        errorMessageSplit.first.substring(1, errorMessageSplit.first.length - 1),
      );
    }

    logger.error('Error trying to sub/unsub to/from subscription stream', ex);
    return ApiError();
  }

  bool _isSimpleRoute(String route) => route.lastIndexOf('/') == 0;

  void _handleSimpleRoute(String route) async {
    if (_currentRoute != null && !_isSimpleRoute(_currentRoute!)) {
      await _unsubscribeFromUpdates(_currentRoute!);
    }
    _currentRoute = route;
    if (!_currentRouteSet.isCompleted) {
      _currentRouteSet.complete();
    }
  }

  // @@TODO: If going to the same route, do nothing.
  void _handleComplexRoute(String route) async {
    route = route.substring(0, route.lastIndexOf('/'));
    if (_currentRoute != null && !_isSimpleRoute(_currentRoute!)) {
      await _unsubThenSubToUpdates(_currentRoute!, route);
    } else {
      await _subscribeToUpdates(route);
    }
    _currentRoute = route;
    if (!_currentRouteSet.isCompleted) {
      _currentRouteSet.complete();
    }
  }

  Future _subscribeToUpdates(String updateStreamIdentifier) async {
    var hubConnection = (await _serverConnector.latestConnection).$1;
    try {
      await hubConnection.invoke(
        'SubscribeToUpdates',
        args: [
          SubscribeToUpdatesCommand(
            updateStreamIdentifier: updateStreamIdentifier,
          ),
        ],
      );
    } on Exception catch (ex) {
      throw _wrapHubException(ex);
    }
  }

  Future _unsubscribeFromUpdates(String updateStreamIdentifier) async {
    var hubConnection = (await _serverConnector.latestConnection).$1;
    try {
      await hubConnection.invoke(
        'UnsubscribeFromUpdates',
        args: [
          UnsubscribeFromUpdatesCommand(
            updateStreamIdentifier: updateStreamIdentifier,
          ),
        ],
      );
    } on Exception catch (ex) {
      throw _wrapHubException(ex);
    }
  }

  Future _unsubThenSubToUpdates(
    String updateStreamIdentifierToUnsub,
    String updateStreamIdentifierToSub,
  ) async {
    var hubConnection = (await _serverConnector.latestConnection).$1;
    try {
      await hubConnection.invoke(
        'UnsubThenSubToUpdates',
        args: [
          UnsubThenSubToUpdatesCommand(
            updateStreamIdentifierToUnsub: updateStreamIdentifierToUnsub,
            updateStreamIdentifierToSub: updateStreamIdentifierToSub,
          ),
        ],
      );
    } on Exception catch (ex) {
      throw _wrapHubException(ex);
    }
  }
}
