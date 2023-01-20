class SignInCommand {
  final String timestamp;
  final String orchestratorSignature;
  final String signature;

  SignInCommand({
    required this.timestamp,
    required this.orchestratorSignature,
    required this.signature,
  });

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['timestamp'] = timestamp;
    map['orchestratorSignature'] = orchestratorSignature;
    map['signature'] = signature;

    return map;
  }
}
