import '../../../ethereum_js_interop.dart';

class SignUpCommand {
  final String email;
  final String confirmationCode;
  final String signatureOverCode;
  final RawAttestation rawAttestation;
  final String keyShare;

  SignUpCommand({
    required this.email,
    required this.confirmationCode,
    required this.signatureOverCode,
    required this.rawAttestation,
    required this.keyShare,
  });

  Map<String, dynamic> toJson() => {
        'email': email,
        'confirmationCode': confirmationCode,
        'signatureOverCode': signatureOverCode,
        'rawAttestation': {
          'id': rawAttestation.id,
          'rawId': rawAttestation.id,
          'type': rawAttestation.type,
          'response': {
            'attestationObject': rawAttestation.response.attestationObject,
            'clientDataJSON': rawAttestation.response.clientDataJSON,
          },
          'extensions': {},
        },
        'keyShare': keyShare,
      };
}
