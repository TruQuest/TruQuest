import 'dart:async';

import '../../ethereum/models/im/user_operation.dart';

class MultiStageOperationContext {
  final approveUserOpTask = Completer<UserOperation?>();
}
