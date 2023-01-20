class SignInRvm {
  final String username;
  final String token;

  SignInRvm.fromMap(Map<String, dynamic> map)
      : username = map['username'],
        token = map['token'];
}
