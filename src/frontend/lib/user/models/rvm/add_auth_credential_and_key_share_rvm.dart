class AddAuthCredentialAndKeyShareRvm {
  final String walletAddress;
  final String token;

  AddAuthCredentialAndKeyShareRvm.fromMap(Map<String, dynamic> map)
      : walletAddress = map['walletAddress'],
        token = map['token'];
}
