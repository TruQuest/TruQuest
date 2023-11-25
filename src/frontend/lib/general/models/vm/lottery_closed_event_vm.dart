class LotteryClosedEventVm {
  final String txnHash;
  final int? nonce;

  LotteryClosedEventVm.fromMap(Map<String, dynamic> map)
      : txnHash = map['txnHash'],
        nonce = map['nonce'];
}
