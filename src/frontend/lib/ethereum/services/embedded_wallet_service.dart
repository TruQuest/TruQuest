import 'dart:convert';
import 'dart:js_util';

import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:rxdart/rxdart.dart';

import '../../general/contexts/multi_stage_operation_context.dart';
import '../../ethereum/services/iwallet_service.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/iframe_manager.dart';
import '../../general/services/local_storage.dart';
import '../../user/errors/get_credential_error.dart';
import '../../user/errors/local_key_share_not_present_error.dart';
import '../../user/services/user_api_service.dart';

class EmbeddedWalletService implements IWalletService {
  final UserApiService _userApiService;
  final IFrameManager _iframeManager;
  final LocalStorage _localStorage;

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

  Future<AttestationOptions> generateConfirmationCodeAndAttestationOptions(String email) =>
      _userApiService.generateConfirmationCodeAndAttestationOptions(email);

  Future<bool> signUp(String email, String confirmationCode, AttestationOptions options) async {
    _onSelectedForOnboarding?.call();

    var result = await promiseToFuture<CreateCredentialResult>(createCredential(options));
    if (result.error != null) {
      print('Error trying to create credential');
      return false;
    }

    var response = await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'gen|$confirmationCode',
    );
    var responseSplit = response.split('|');
    var keyShare = responseSplit[0];
    var signatureOverCode = responseSplit[1];

    var signUpResult = await _userApiService.signUp(
      email,
      dotenv.env['ENVIRONMENT'] == 'Development' ? confirmationCode.split(' ').first : confirmationCode,
      signatureOverCode,
      result.attestation!,
      keyShare,
    );

    var signerAddress = convertToEip55Address(signUpResult.signerAddress);
    var walletAddress = convertToEip55Address(signUpResult.walletAddress);

    await _localStorage.setString(
      'Wallet',
      jsonEncode({
        'name': name,
        'signerAddress': signerAddress,
        signerAddress: {
          'userId': signUpResult.userId,
          'walletAddress': walletAddress,
          'token': signUpResult.token,
        },
      }),
    );

    _currentSignerChangedEventChannel.add(signerAddress);

    await _iframeManager.iframeKeyShareRender.postMessageAndAwaitResponse('render');

    return true;
  }

  void saveKeyShareQrCodeImage() async {
    var result = await _iframeManager.iframeKeyShareRender.postMessageAndAwaitResponse('save');
    print('Save key share image result: $result');
  }

  Stream<Object> signInFromExistingDevice(MultiStageOperationContext ctx) async* {
    var options = await _userApiService.generateAssertionOptionsForSignIn();
    var result = await promiseToFuture<GetCredentialResult>(getCredential(options));
    if (result.error != null) {
      print('Error trying to get credential');
      yield const GetCredentialError();
      return;
    }

    var signInResult = await _userApiService.verifyAssertionAndSignIn(result.assertion!);

    var scanRequestId = DateTime.now().millisecondsSinceEpoch.toString();
    var present = await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'check-local-key-share|$scanRequestId',
    );
    if (present == '0') {
      yield LocalKeyShareNotPresentError(scanRequestId: scanRequestId);
      await ctx.scanQrCodeTask.future;

      present = await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
        'check-local-key-share-again|$scanRequestId',
      );
      if (present == '0') {
        yield const LocalKeyShareNotPresentError();
        return;
      }
    }

    _onSelectedForOnboarding?.call();

    var signerAddress = convertToEip55Address(signInResult.signerAddress);
    var walletAddress = convertToEip55Address(signInResult.walletAddress);

    await _localStorage.setString(
      'Wallet',
      jsonEncode({
        'name': name,
        'signerAddress': signerAddress,
        signerAddress: {
          'userId': signInResult.userId,
          'walletAddress': walletAddress,
          'token': signInResult.token,
        },
      }),
    );

    _currentSignerChangedEventChannel.add(signerAddress);
  }

  @override
  Future<String> personalSign(String message) async {
    var options = await _userApiService.generateAssertionOptions();
    var result = await promiseToFuture<GetCredentialResult>(getCredential(options));
    if (result.error != null) {
      print('Error trying to get credential');
      throw GetCredentialError();
    }

    var serverKeyShare = await _userApiService.verifyAssertionAndGetKeyShare(result.assertion!);

    return await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'sign-message|$serverKeyShare|$message',
    );
  }

  @override
  Future<String> personalSignDigest(String digest) async {
    var options = await _userApiService.generateAssertionOptions();
    var result = await promiseToFuture<GetCredentialResult>(getCredential(options));
    if (result.error != null) {
      print('Error trying to get credential');
      throw GetCredentialError();
    }

    var serverKeyShare = await _userApiService.verifyAssertionAndGetKeyShare(result.assertion!);

    return await _iframeManager.iframePrivateKeyGen.postMessageAndAwaitResponse(
      'sign-digest|$serverKeyShare|$digest',
    );
  }
}
