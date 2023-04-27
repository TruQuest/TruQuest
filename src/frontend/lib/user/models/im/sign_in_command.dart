class SignInCommand {
  final String timestamp;
  final String orchestratorSignature;
  final String signature;

  SignInCommand({
    required this.timestamp,
    required this.orchestratorSignature,
    required this.signature,
  });

  Map<String, dynamic> toJson() => {
        'timestamp': timestamp,
        'orchestratorSignature': orchestratorSignature,
        'signature': signature,
      };
}
