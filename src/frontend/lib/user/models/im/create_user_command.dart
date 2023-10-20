class CreateUserCommand {
  final String email;

  CreateUserCommand({required this.email});

  Map<String, dynamic> toJson() => {'email': email};
}
