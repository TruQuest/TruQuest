import 'error.dart';

class UnhandledError extends Error {
  final String traceId;

  const UnhandledError(super.message, this.traceId);
}
