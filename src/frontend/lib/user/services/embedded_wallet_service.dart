import 'dart:convert';
import 'dart:js_util';

import 'package:rxdart/rxdart.dart';

import 'user_api_service.dart';
import '../../ethereum/services/iwallet_service.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/iframe_manager.dart';
import '../../general/services/local_storage.dart';

class EmbeddedWalletService implements IWalletService {
  final UserApiService _userApiService;
  final IFrameManager _iframeManager;
  final LocalStorage _localStorage;

  String get privateKeyGenIframeViewId => _iframeManager.iframePrivateKeyGen.viewId;
  String get qrCodeScanIframeViewId => _iframeManager.iframeQrCodeScan.viewId;

  void Function()? _onSelectedForOnboarding;
  set onSelectedForOnboarding(void Function() f) => _onSelectedForOnboarding = f;

  @override
  String get name => 'Embedded';

  final _currentSignerChangedEventChannel = BehaviorSubject<String?>();
  @override
  Stream<String?> get currentSignerChanged$ => _currentSignerChangedEventChannel.stream;

  EmbeddedWalletService(
    this._userApiService,
    this._iframeManager,
    this._localStorage,
  );

  void setup(Map<String, dynamic> wallet) => _currentSignerChangedEventChannel.add(wallet['signerAddress']);

  Future<AttestationOptions> generateConfirmationCodeAndAttestationOptions(String email) {
    return _userApiService.generateConfirmationCodeAndAttestationOptions(email);
  }

  Future<bool> signUp(String email, String confirmationCode, AttestationOptions options) async {
    _onSelectedForOnboarding?.call();

    var attestation = await promiseToFuture<RawAttestation>(createCredential(options));
    var response = await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'gen|$confirmationCode',
    );
    var responseSplit = response.split('|');
    var keyShare = responseSplit[0];
    var signatureOverCode = responseSplit[1];

    var result = await _userApiService.signUp(
      email,
      confirmationCode,
      signatureOverCode,
      attestation,
      keyShare,
    );

    var signerAddress = convertToEip55Address(result.signerAddress);
    var walletAddress = convertToEip55Address(result.walletAddress);

    await _localStorage.setString(
      'Wallet',
      jsonEncode({
        'name': name,
        'signerAddress': signerAddress,
        signerAddress: {
          'userId': result.userId,
          'walletAddress': walletAddress,
          'token': result.token,
        },
      }),
    );

    _currentSignerChangedEventChannel.add(signerAddress);

    await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse('render');

    return true;
  }

  @override
  Future<String> personalSign(String message) async => throw UnimplementedError();

  @override
  Future<String> personalSignDigest(String digest) async {
    var options = await _userApiService.generateAssertionOptions();
    var assertion = await promiseToFuture<RawAssertion>(getCredential(options));
    var serverKeyShare = await _userApiService.verifyAssertionAndGetKeyShare(assertion);

    return await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'sign-digest|$serverKeyShare|$digest',
    );
  }
}
