class ConfirmEmailAndGetAttestationOptionsCommand {
  final String email;
  final String confirmationCode;

  ConfirmEmailAndGetAttestationOptionsCommand({
    required this.email,
    required this.confirmationCode,
  });

  Map<String, dynamic> toJson() => {
        'email': email,
        'confirmationCode': confirmationCode,
      };
}
