class ConfirmEmailCommand {
  final String confirmationToken;

  ConfirmEmailCommand({required this.confirmationToken});

  Map<String, dynamic> toJson() => {
        'confirmationToken': confirmationToken,
      };
}
