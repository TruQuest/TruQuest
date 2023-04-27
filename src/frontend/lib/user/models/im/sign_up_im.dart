class SignUpIm {
  final String username;

  SignUpIm({required this.username});

  Map<String, dynamic> toJson() => {'username': username};
}
