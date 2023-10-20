import '../../../ethereum_js_interop.dart';

class AddAuthCredentialAndKeyShareCommand {
  final RawAttestation rawAttestation;
  final String nonce;
  final String signatureOverNonce;
  final String keyShare;

  AddAuthCredentialAndKeyShareCommand({
    required this.rawAttestation,
    required this.nonce,
    required this.signatureOverNonce,
    required this.keyShare,
  });

  Map<String, dynamic> toJson() => {
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
        'nonce': nonce,
        'signatureOverNonce': signatureOverNonce,
        'keyShare': keyShare,
      };
}
