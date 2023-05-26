class SignInWithEthereumCommand {
  final String message;
  final String signature;

  SignInWithEthereumCommand({
    required this.message,
    required this.signature,
  });

  Map<String, dynamic> toJson() => {
        'message': message,
        'signature': signature,
      };
}
