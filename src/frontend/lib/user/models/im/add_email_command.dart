class AddEmailCommand {
  final String email;

  AddEmailCommand({required this.email});

  Map<String, dynamic> toJson() => {
        'email': email,
      };
}
