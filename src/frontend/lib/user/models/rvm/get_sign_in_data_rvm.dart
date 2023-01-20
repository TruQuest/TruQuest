class GetSignInDataRvm {
  final String timestamp;
  final String signature;

  GetSignInDataRvm.fromMap(Map<String, dynamic> map)
      : timestamp = map['timestamp'],
        signature = map['signature'];
}
