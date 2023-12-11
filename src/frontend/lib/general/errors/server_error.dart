import 'error.dart';

class ServerError extends Error {
  ServerError({String message = 'Something went wrong on the server. Try again later'}) : super(message);
}
