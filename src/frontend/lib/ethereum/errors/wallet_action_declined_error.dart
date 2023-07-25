import '../../general/errors/error.dart';

class WalletActionDeclinedError extends Error {
  const WalletActionDeclinedError() : super('Action declined');
}
