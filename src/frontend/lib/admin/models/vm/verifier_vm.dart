class VerifierVm {
  final String userId;
  final String walletAddress;

  VerifierVm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        walletAddress = map['walletAddress'];
}
