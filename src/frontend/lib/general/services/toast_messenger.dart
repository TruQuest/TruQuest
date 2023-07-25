import 'dart:async';

class ToastMessenger {
  final _toastChannel = StreamController<String>();
  Stream<String> get toast$ => _toastChannel.stream;

  void add(String toast) => _toastChannel.add(toast);
}
