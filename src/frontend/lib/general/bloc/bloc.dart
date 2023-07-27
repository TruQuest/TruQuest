import 'dart:async';

import '../errors/validation_error.dart';
import 'actions.dart';
import '../services/toast_messenger.dart';
import '../contexts/multi_stage_operation_context.dart';

abstract class Bloc<TAction extends Action> {
  final ToastMessenger toastMessenger;

  final actionChannel = StreamController<TAction>();

  Bloc(this.toastMessenger);

  void dispatch(TAction action) {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return;
    }

    actionChannel.add(action);
  }

  Future<TResult?> execute<TResult>(TAction action) async {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return null;
    }

    return await handleExecute(action) as TResult;
  }

  Future<Object?> handleExecute(TAction action) => throw UnimplementedError();

  Stream<Object> executeMultiStage(
    TAction action,
    MultiStageOperationContext ctx,
  ) {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return Stream<Object>.value(const ValidationError());
    }

    return handleMultiStageExecute(action, ctx);
  }

  Stream<Object> handleMultiStageExecute(
    TAction action,
    MultiStageOperationContext ctx,
  ) =>
      throw UnimplementedError();

  void dispose({TAction? cleanupAction}) {}
}
