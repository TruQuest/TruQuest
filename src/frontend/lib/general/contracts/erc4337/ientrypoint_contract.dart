import '../../../ethereum/models/im/user_operation.dart';
import '../../../ethereum_js_interop.dart';

abstract class IEntryPointContract {
  String get address;
  String get userOperationEventName;
  String get userOperationRevertReasonEventName;
  Future<BigInt> getNonce(String sender);
  Future<String> getUserOpHash(UserOperation userOp);
  LogDescription parseLog(List<String> topics, String data);
}
