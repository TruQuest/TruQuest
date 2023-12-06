class UserOperationReceiptVm {
  final String blockHash;
  final int blockNumber;
  final String transactionHash;

  UserOperationReceiptVm.fromMap(Map<String, dynamic> map)
      : blockHash = map['blockHash'],
        blockNumber = int.parse(map['blockNumber']),
        transactionHash = map['transactionHash'];

  Map<String, dynamic> toJson() => {
        'blockHash': blockHash,
        'blockNumber': blockNumber,
        'transactionHash': transactionHash,
      };
}
