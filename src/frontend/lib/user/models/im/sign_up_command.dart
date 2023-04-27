import 'sign_up_im.dart';

class SignUpCommand {
  final SignUpIm input;
  final String signature;

  SignUpCommand({
    required this.input,
    required this.signature,
  });

  Map<String, dynamic> toJson() => {
        'input': input.toJson(),
        'signature': signature,
      };
}
