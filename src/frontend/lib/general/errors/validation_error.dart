import 'error.dart';

class ValidationError extends Error {
  const ValidationError() : super('Invalid input data');
}
