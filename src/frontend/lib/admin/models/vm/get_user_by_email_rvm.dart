class GetUserByEmailRvm {
  final String userId;
  final String walletAddress;
  final String signerAddress;
  final bool emailConfirmed;

  GetUserByEmailRvm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        walletAddress = map['walletAddress'],
        signerAddress = map['signerAddress'],
        emailConfirmed = map['emailConfirmed'];
}
