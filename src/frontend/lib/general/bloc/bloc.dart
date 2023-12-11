import 'dart:async';

import 'actions.dart';
import '../utils/logger.dart';
import '../errors/validation_error.dart';
import '../services/toast_messenger.dart';
import '../contexts/multi_stage_operation_context.dart';
import '../errors/failed_multi_stage_execution_error.dart';

abstract class Bloc<TAction extends Action> {
  final ToastMessenger toastMessenger;

  final _actionChannel = StreamController<TAction>();
  late final Future Function(TAction action) onAction;

  Bloc(this.toastMessenger) {
    _actionChannel.stream.listen((action) async {
      try {
        await onAction(action);
      } catch (e) {
        logger.warning('Error trying to handle action: $action');
      }
    });
  }

  void dispatch(TAction action) {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return;
    }

    _actionChannel.add(action);
  }

  Future<TResult?> execute<TResult>(TAction action) async {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      return null;
    }

    try {
      return await handleExecute(action) as TResult;
    } catch (e) {
      logger.warning('Error trying to handle action: $action');
      return null;
    }
  }

  Future<Object?> handleExecute(TAction action) => throw UnimplementedError();

  Stream<Object> executeMultiStage(TAction action, MultiStageOperationContext ctx) async* {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      yield const ValidationError();
      return;
    }

    try {
      yield* handleMultiStageExecute(action, ctx);
    } catch (e) {
      logger.warning('Error trying to handle action: $action');
      yield const FailedMultiStageExecutionError();
    }
  }

  Stream<Object> handleMultiStageExecute(TAction action, MultiStageOperationContext ctx) => throw UnimplementedError();

  void dispose({TAction? cleanupAction}) {}
}
