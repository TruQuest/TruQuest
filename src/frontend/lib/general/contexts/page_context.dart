import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';

class PageContext {
  late final PageController controller;
  final _routeChannel = BehaviorSubject<String>();
  Stream<String> get route$ => _routeChannel.stream;

  String get currentRoute => _routeChannel.value;

  PageContext() {
    controller = PageController(initialPage: 0);
    _routeChannel.add('/subjects');
  }

  void goto(String route) {
    _routeChannel.add(route);
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

    controller.jumpToPage(page);
  }
}
