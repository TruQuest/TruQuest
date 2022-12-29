class SignUpIm {
  final String username;

  SignUpIm({required this.username});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};
    map['username'] = username;

    return map;
  }
}
