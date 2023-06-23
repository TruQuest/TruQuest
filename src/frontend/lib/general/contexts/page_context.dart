import 'package:rxdart/rxdart.dart';

import '../services/local_storage.dart';

class PageContext {
  final LocalStorage _localStorage;

  final _routeChannel = BehaviorSubject<String>();
  Stream<String> get route$ => _routeChannel.stream;

  String get currentRoute => _routeChannel.value;

  PageContext(this._localStorage);

  Future init() async {
    String initialRoute = '/subjects';
    var currentRoute = _localStorage.getString('currentRoute');
    if (currentRoute != null) {
      initialRoute = currentRoute;
    }

    await _saveCurrentRoute(initialRoute);
    _routeChannel.add(initialRoute);
  }

  Future _saveCurrentRoute(String route) =>
      _localStorage.setString('currentRoute', route);

  void goto(String route) async {
    _routeChannel.add(route);
    await _saveCurrentRoute(route);
  }
}
