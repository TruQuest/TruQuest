import 'dart:async';

abstract class IWalletService {
  String get name;
  Stream<String?> get currentSignerChanged$;
  Future<String> personalSign(String message);
  Future<String> personalSignDigest(String digest);
}
