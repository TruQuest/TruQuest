import 'dart:async';

import 'actions.dart';
import '../utils/logger.dart';
import '../errors/error.dart';
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
        logger.warning('Error trying to handle action $action: $e');
        toastMessenger.add(e is Error ? e.toString() : 'Sorry, something went wrong. This shouldn\'t have happened');
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
      logger.warning('Error trying to handle action $action: $e');
      toastMessenger.add(e is Error ? e.toString() : 'Sorry, something went wrong. This shouldn\'t have happened');
      return null;
    }
  }

  Future<Object?> handleExecute(TAction action) => throw UnimplementedError();

  Stream<Object> executeMultiStage(TAction action, MultiStageOperationContext ctx) async* {
    List<String>? validationErrors;
    if ((validationErrors = action.validate()) != null) {
      toastMessenger.add('• ' + validationErrors!.join('\n• '));
      // @@??!!: Why the hell synchronous return makes Flutter go nuts??
      await Future.delayed(const Duration(milliseconds: 50));
      yield const ValidationError();
      return;
    }

    try {
      // @@BUG: Flutter Web issue: 'catch' block is never executed, just hangs when exception is thrown.
      // See: https://github.com/dart-lang/sdk/issues/47764
      yield* handleMultiStageExecute(action, ctx);
    } catch (e) {
      logger.warning('Error trying to handle action $action: $e');
      toastMessenger.add(e is Error ? e.toString() : 'Sorry, something went wrong. This shouldn\'t have happened');
      yield const FailedMultiStageExecutionError();
    }
  }

  Stream<Object> handleMultiStageExecute(TAction action, MultiStageOperationContext ctx) => throw UnimplementedError();

  void dispose({TAction? cleanupAction}) {}
}
