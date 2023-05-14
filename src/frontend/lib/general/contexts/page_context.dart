import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';

import '../services/local_storage.dart';

class PageContext {
  final LocalStorage _localStorage;

  late final PageController _controller;
  final _routeChannel = BehaviorSubject<String>();
  Stream<String> get route$ => _routeChannel.stream;

  String get currentRoute => _routeChannel.value;

  PageContext(this._localStorage);

  Future<PageController> init() async {
    await _localStorage.init();

    int initialPage = 0;
    String initialRoute = '/subjects';

    var currentRouteTs = _localStorage.getString('currentRoute');
    if (currentRouteTs != null) {
      var currentRouteTsSplit = currentRouteTs.split('::');
      var route = currentRouteTsSplit.first;
      var tsMs = int.parse(currentRouteTsSplit.last);
      var ts = DateTime.fromMillisecondsSinceEpoch(tsMs);
      if (DateTime.now().difference(ts).inMinutes < 10) {
        initialPage = _getPageIndex(route);
        initialRoute = route;
      }
    }

    await _saveCurrentRoute(initialRoute);

    _controller = PageController(initialPage: initialPage);
    _routeChannel.add(initialRoute);

    return _controller;
  }

  int _getPageIndex(String route) {
    int page;
    switch (route) {
      case '/subjects':
        page = 0;
        break;
      case '/things':
        page = 1;
        break;
      case '/how-to':
        page = 2;
        break;
      case '/pong':
        page = 3;
        break;
      case '/goto':
        page = 4;
        break;
      default:
        page = DateTime.now().millisecondsSinceEpoch;
        break;
    }

    return page;
  }

  Future _saveCurrentRoute(String route) async {
    await _localStorage.setString(
      'currentRoute',
      '$route::${DateTime.now().millisecondsSinceEpoch}',
    );
  }

  void goto(String route) async {
    _routeChannel.add(route);
    _controller.jumpToPage(_getPageIndex(route));
    await _saveCurrentRoute(route);
  }
}
