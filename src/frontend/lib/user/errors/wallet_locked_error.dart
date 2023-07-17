import '../../general/errors/error.dart';

class WalletLockedError extends Error {
  WalletLockedError() : super('Wallet locked');
}
