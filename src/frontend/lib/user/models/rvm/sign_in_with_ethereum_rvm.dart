class SignInWithEthereumRvm {
  final String username;
  final String token;

  SignInWithEthereumRvm.fromMap(Map<String, dynamic> map)
      : username = map['username'],
        token = map['token'];
}
