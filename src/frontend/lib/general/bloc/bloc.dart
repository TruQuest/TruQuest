import 'dart:async';

abstract class Bloc<TAction> {
  StreamController<TAction> actionChannel = StreamController<TAction>();

  void dispatch(TAction action) {
    actionChannel.add(action);
  }

  void dispose({TAction? cleanupAction}) {}
}
