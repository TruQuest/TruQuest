import '../contexts/page_context.dart';
import '../models/im/unsub_then_sub_to_updates_command.dart';
import '../models/im/unsubscribe_from_updates_command.dart';
import 'server_connector.dart';
import '../errors/error.dart';
import '../errors/server_error.dart';
import '../errors/validation_error.dart';
import '../models/im/subscribe_to_updates_command.dart';

class SubscriptionManager {
  final PageContext _pageContext;
  final ServerConnector _serverConnector;

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
  }

  Error _wrapHubException(Exception ex) {
    var errorMessage = ex.toString();
    if (errorMessage.contains('[ValidationError]')) {
      return ValidationError();
    }

    print(ex);

    return ServerError();
  }

  bool _isSimpleRoute(String route) => route.lastIndexOf('/') == 0;

  void _handleSimpleRoute(String route) async {
    if (_currentRoute != null && !_isSimpleRoute(_currentRoute!)) {
      await _unsubscribeFromUpdates(_currentRoute!);
    }
    _currentRoute = route;
  }

  void _handleComplexRoute(String route) async {
    if (_currentRoute != null && !_isSimpleRoute(_currentRoute!)) {
      await _unsubThenSubToUpdates(_currentRoute!, route);
    } else {
      await _subscribeToUpdates(route);
    }
    _currentRoute = route;
  }

  Future _subscribeToUpdates(String updateStreamIdentifier) async {
    var hubConnection = _serverConnector.hubConnection;
    if (hubConnection == null) {
      print('Not connected to hub!');
      return;
    }

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
    var hubConnection = _serverConnector.hubConnection;
    if (hubConnection == null) {
      print('Not connected to hub!');
      return;
    }

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
    var hubConnection = _serverConnector.hubConnection;
    if (hubConnection == null) {
      print('Not connected to hub!');
      return;
    }

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
