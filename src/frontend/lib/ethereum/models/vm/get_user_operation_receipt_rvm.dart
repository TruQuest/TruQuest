import 'dart:convert';

import 'event_vm.dart';
import 'user_operation_receipt_vm.dart';

class GetUserOperationReceiptRvm {
  final int nonce;
  final int actualGasCost;
  final int actualGasUsed;
  final bool success;
  final String? reason;
  final List<EventVm> logs;
  final UserOperationReceiptVm receipt;

  GetUserOperationReceiptRvm.fromMap(Map<String, dynamic> map)
      : nonce = int.parse(map['nonce']),
        actualGasCost = int.parse(map['actualGasCost']),
        actualGasUsed = int.parse(map['actualGasUsed']),
        success = map['success'],
        reason = map.containsKey('reason') ? map['reason'] : null,
        logs = List.unmodifiable((map['logs'] as List<dynamic>).map((submap) => EventVm.fromMap(submap))),
        receipt = UserOperationReceiptVm.fromMap(map['receipt']);

  Map<String, dynamic> toJson() => {
        'nonce': nonce,
        'actualGasCost': actualGasCost,
        'actualGasUsed': actualGasUsed,
        'success': success,
        'reason': reason,
        'logs': List.generate(logs.length, (_) => '...', growable: false),
        'receipt': receipt.toJson(),
      };

  @override
  String toString() => const JsonEncoder.withIndent('  ').convert(toJson());
}
