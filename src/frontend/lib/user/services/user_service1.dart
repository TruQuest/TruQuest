import 'dart:js_util';

import '../../ethereum_js_interop.dart';
import '../../general/services/iframe_manager.dart';
import 'user_api_service.dart';

class UserService1 {
  final UserApiService _userApiService;
  final IFrameManager _iframeManager;

  String get privateKeyGenIframeViewId => _iframeManager.iframePrivateKeyGen.viewId;
  String get qrCodeScanIframeViewId => _iframeManager.iframeQrCodeScan.viewId;

  UserService1(
    this._userApiService,
    this._iframeManager,
  );

  Future createUser(String email) async {
    await _userApiService.createUser(email);
  }

  Future<String> confirmEmailAndGetAttestationOptions(String email, String confirmationCode) async {
    var result = await _userApiService.confirmEmailAndGetAttestationOptions(email, confirmationCode);
    var attestation = await promiseToFuture<RawAttestation>(createCredential(result.options));

    var response = await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'gen|${result.nonce}',
    );
    var responseSplit = response.split('|');
    var keyShare = responseSplit[0];
    var signatureOverNonce = responseSplit[1];

    var signUpResult = await _userApiService.addAuthCredentialAndKeyShare(
      attestation,
      result.nonce,
      signatureOverNonce,
      keyShare,
    );

    await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse('render');

    return _iframeManager.iframePrivateKeyGen.viewId;
  }
}
