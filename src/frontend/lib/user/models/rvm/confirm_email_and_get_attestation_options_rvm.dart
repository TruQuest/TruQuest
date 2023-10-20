import '../../../ethereum_js_interop.dart';

class ConfirmEmailAndGetAttestationOptionsRvm {
  final AttestationOptions options;
  final String nonce;

  ConfirmEmailAndGetAttestationOptionsRvm.fromMap(Map<String, dynamic> map)
      : options = AttestationOptions(
          rp: RelyingParty(
            id: map['options']['rp']['id'],
            name: map['options']['rp']['name'],
          ),
          user: User(
            id: map['options']['user']['id'],
            name: map['options']['user']['name'],
            displayName: map['options']['user']['displayName'],
          ),
          challenge: map['options']['challenge'],
          pubKeyCredParams: (map['options']['pubKeyCredParams'] as List<dynamic>)
              .map(
                (submap) => PubKeyCredParam(
                  type: submap['type'],
                  alg: submap['alg'],
                ),
              )
              .toList(),
          timeout: map['options']['timeout'],
          attestation: map['options']['attestation'],
          authenticatorSelection: AuthenticatorSelection(
            authenticatorAttachment: map['options']['authenticatorSelection']['authenticatorAttachment'],
            residentKey: map['options']['authenticatorSelection']['residentKey'],
            requireResidentKey: map['options']['authenticatorSelection']['requireResidentKey'],
            userVerification: map['options']['authenticatorSelection']['userVerification'],
          ),
          excludeCredentials: (map['options']['excludeCredentials'] as List<dynamic>)
              .map(
                (submap) => PublicKeyCredentialDescriptor(
                  type: submap['type'],
                  id: submap['id'],
                  transports: submap.containsKey('transports') ? submap['transports'] : null,
                ),
              )
              .toList(),
          // extensions: Extensions(
          //   prf: Prf(
          //     eval: Eval(
          //       first: map['options']['extensions']['prf']['eval']['first'],
          //     ),
          //   ),
          // ),
        ),
        nonce = map['nonce'];
}
