class GenerateConfirmationCodeAndAttestationOptionsCommand {
  final String email;

  GenerateConfirmationCodeAndAttestationOptionsCommand({required this.email});

  Map<String, dynamic> toJson() => {'email': email};
}
