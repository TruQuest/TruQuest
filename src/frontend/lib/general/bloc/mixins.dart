import 'dart:async';

mixin AwaitableResult<T> {
  final Completer<T> _resultReady = Completer<T>();
  Future<T> get result => _resultReady.future;

  void complete(T result) => _resultReady.complete(result);
}
