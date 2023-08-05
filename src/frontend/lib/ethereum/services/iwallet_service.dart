import 'dart:async';

abstract class IWalletService {
  Stream<(String?, String?)> get currentWalletAddressChanged$;
  String? get currentWalletAddress;
  String? get currentOwnerAddress;
  bool get isUnlocked;
  FutureOr<String> personalSign(String message);
  FutureOr<String> personalSignDigest(String digest);
}
