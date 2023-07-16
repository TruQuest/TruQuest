import '../../../ethereum/models/im/user_operation.dart';

abstract class IEntryPointContract {
  String get address;
  Future<BigInt> getNonce(String sender);
  Future<String> getUserOpHash(UserOperation userOp);
}
