class FundWithEthAndTruRvm {
  final String? ethTxnHash;
  final String? truTxnHash;

  FundWithEthAndTruRvm.fromMap(Map<String, dynamic> map)
      : ethTxnHash = map['ethTxnHash'],
        truTxnHash = map['truTxnHash'];
}
