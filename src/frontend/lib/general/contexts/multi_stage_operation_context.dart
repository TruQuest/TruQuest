import 'dart:async';

import '../../ethereum/models/im/user_operation.dart';

class MultiStageOperationContext {
  final unlockWalletTask = Completer<bool>();
  final approveUserOpTask = Completer<UserOperation?>();
}
