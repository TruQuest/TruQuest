import 'sign_up_im.dart';

class SignUpCommand {
  final SignUpIm input;
  final String signature;

  SignUpCommand({
    required this.input,
    required this.signature,
  });

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['input'] = input.toJson();
    map['signature'] = signature;

    return map;
  }
}
