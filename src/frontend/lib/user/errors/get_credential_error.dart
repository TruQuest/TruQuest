import '../../general/errors/error.dart';

class GetCredentialError extends Error {
  const GetCredentialError() : super('Error trying to authenticate with passkey');
}
