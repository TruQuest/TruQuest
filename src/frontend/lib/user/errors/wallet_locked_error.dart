import '../../general/errors/error.dart';

class WalletLockedError extends Error {
  const WalletLockedError() : super('Wallet locked');
}
