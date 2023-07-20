class UserOperationReceiptVm {
  final String blockHash;
  final int blockNumber;
  final String transactionHash;
  final int confirmations;

  UserOperationReceiptVm.fromMap(Map<String, dynamic> map)
      : blockHash = map['blockHash'],
        blockNumber = int.parse(map['blockNumber']),
        transactionHash = map['transactionHash'],
        confirmations = int.parse(map['confirmations']);

  Map<String, dynamic> toJson() => {
        'blockHash': blockHash,
        'blockNumber': blockNumber,
        'transactionHash': transactionHash,
        'confirmations': confirmations,
      };
}
