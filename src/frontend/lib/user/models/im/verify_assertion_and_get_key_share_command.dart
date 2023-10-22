import '../../../ethereum_js_interop.dart';

class VerifyAssertionAndGetKeyShareCommand {
  final RawAssertion rawAssertion;

  VerifyAssertionAndGetKeyShareCommand({required this.rawAssertion});

  Map<String, dynamic> toJson() => {
        'rawAssertion': {
          'id': rawAssertion.id,
          'rawId': rawAssertion.id,
          'type': rawAssertion.type,
          'response': {
            'authenticatorData': rawAssertion.response.authenticatorData,
            'clientDataJSON': rawAssertion.response.clientDataJSON,
            'signature': rawAssertion.response.signature,
          },
          'extensions': {},
        }
      };
}
