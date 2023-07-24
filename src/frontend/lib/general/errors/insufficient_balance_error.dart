import 'error.dart';

class InsufficientBalanceError extends Error {
  const InsufficientBalanceError() : super('Insufficient balance');
}
