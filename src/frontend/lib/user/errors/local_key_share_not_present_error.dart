import '../../general/errors/error.dart';

class LocalKeyShareNotPresentError extends Error {
  final String? scanRequestId;

  const LocalKeyShareNotPresentError({this.scanRequestId}) : super('Local key share is absent');
}
