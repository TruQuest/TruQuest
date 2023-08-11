import '../im/user_operation.dart';
import '../../../general/utils/utils.dart';

class UserOperationVm {
  final UserOperation userOp;

  final String _sender;
  final String functionSignature;
  final String description;
  final BigInt? _stakeSize;
  final BigInt _estimatedGas;
  final BigInt _estimatedTxnFee;
  final bool txnFeeCoveredByPaymaster;

  UserOperationVm(
    this.userOp,
    this._sender,
    this.functionSignature,
    this.description,
    this._stakeSize,
    this._estimatedGas,
    this._estimatedTxnFee,
    this.txnFeeCoveredByPaymaster,
  );

  String get sender => _sender;
  String get senderShort => '${_sender.substring(0, 8)}..${_sender.substring(_sender.length - 6)}';
  bool get hasStake => _stakeSize != null;
  String get stakeSizeShort => getFixedLengthAmount(_stakeSize!, 'TRU') + ' TRU';
  String get stakeSize => getMinLengthAmount(_stakeSize!, 'TRU') + ' TRU';
  String get estimatedGas => _estimatedGas.toStringWithSpaces();
  String get estimatedTxnFeeShort => getFixedLengthAmount(_estimatedTxnFee, 'ETH') + ' ETH';
  String get estimatedTxnFee => getMinLengthAmount(_estimatedTxnFee, 'ETH') + ' ETH';
}
