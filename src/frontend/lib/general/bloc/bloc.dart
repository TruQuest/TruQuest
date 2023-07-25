import 'dart:async';

import '../errors/validation_error.dart';
import 'actions.dart';
import '../services/toast_messenger.dart';
import '../contexts/multi_stage_operation_context.dart';

abstract class Bloc<TAction extends Action> {
  final ToastMessenger _toastMessenger;

  final actionChannel = StreamController<TAction>();

  Bloc(this._toastMessenger);

  void dispatch(TAction action) {
    List<String>? validationErrors;
    if (action.mustValidate && (validationErrors = action.validate()) != null) {
      _toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return;
    }

    actionChannel.add(action);
  }

  Future<Object?> execute(TAction action) {
    List<String>? validationErrors;
    if (action.mustValidate && (validationErrors = action.validate()) != null) {
      _toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return Future.value(null);
    }

    return handle(action);
  }

  Future<Object?> handle(TAction action) => throw UnimplementedError();

  Stream<Object> executeMultiStage(
    TAction action,
    MultiStageOperationContext ctx,
  ) {
    List<String>? validationErrors;
    if (action.mustValidate && (validationErrors = action.validate()) != null) {
      _toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return Stream<Object>.value(const ValidationError());
    }

    return handleMultiStage(action, ctx);
  }

  Stream<Object> handleMultiStage(
    TAction action,
    MultiStageOperationContext ctx,
  ) =>
      throw UnimplementedError();

  void dispose({TAction? cleanupAction}) {}
}
