import 'dart:async';

import 'package:flutter/material.dart';

class ToastMessenger {
  final StreamController<Widget> _toastChannel = StreamController<Widget>();
  Stream<Widget> get toast$ => _toastChannel.stream;

  void add(Widget toast) => _toastChannel.add(toast);
}
