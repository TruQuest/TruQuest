class AuthRvm {
  final String userId;
  final String signerAddress;
  final String walletAddress;
  final String token;

  AuthRvm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        signerAddress = map['signerAddress'],
        walletAddress = map['walletAddress'],
        token = map['token'];
}
