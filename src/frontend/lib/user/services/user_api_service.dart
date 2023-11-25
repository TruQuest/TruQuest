import 'package:dio/dio.dart';

import '../../ethereum_js_interop.dart';
import '../../general/utils/utils.dart';
import '../models/im/generate_confirmation_code_and_attestation_options_command.dart';
import '../models/im/sign_in_with_ethereum_command.dart';
import '../models/im/sign_up_command.dart';
import '../models/im/verify_assertion_and_get_key_share_command.dart';
import '../models/im/verify_assertion_and_sign_in_command.dart';
import '../models/vm/auth_rvm.dart';
import '../models/im/mark_notifications_as_read_command.dart';
import '../models/im/notification_im.dart';
import '../models/im/watched_item_type_im.dart';
import '../../general/models/vm/notification_vm.dart';
import '../../general/services/server_connector.dart';

class UserApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  UserApiService(this._serverConnector) : _dio = _serverConnector.dio;

  Future<String> getNonceForSiwe(String account) async {
    try {
      var response = await _dio.get('/user/siwe/$account');
      return response.data['data'] as String;
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<AuthRvm> signInWithEthereum(String message, String signature) async {
    try {
      var response = await _dio.post(
        '/user/siwe',
        data: SignInWithEthereumCommand(
          message: message,
          signature: signature,
        ).toJson(),
      );

      return AuthRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future markNotificationsAsRead(List<NotificationVm> notifications) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/user/watch-list',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: MarkNotificationsAsReadCommand(
          notifications: notifications
              .map(
                (n) => NotificationIm(
                  updateTimestamp: n.updateTimestamp,
                  itemType: WatchedItemTypeIm.values[n.itemType.index],
                  itemId: n.itemId,
                  itemUpdateCategory: n.itemUpdateCategory,
                ),
              )
              .toList(),
        ).toJson(),
      );
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<AttestationOptions> generateConfirmationCodeAndAttestationOptions(String email) async {
    try {
      var response = await _dio.post(
        '/user/generate-code-and-attestation-options',
        data: GenerateConfirmationCodeAndAttestationOptionsCommand(email: email).toJson(),
      );

      var map = response.data['data'];

      return AttestationOptions(
        rp: RelyingParty(
          id: map['rp']['id'],
          name: map['rp']['name'],
        ),
        user: User(
          id: map['user']['id'],
          name: map['user']['name'],
          displayName: map['user']['displayName'],
        ),
        challenge: map['challenge'],
        pubKeyCredParams: (map['pubKeyCredParams'] as List<dynamic>)
            .map(
              (submap) => PubKeyCredParam(
                type: submap['type'],
                alg: submap['alg'],
              ),
            )
            .toList(),
        timeout: map['timeout'],
        attestation: map['attestation'],
        authenticatorSelection: AuthenticatorSelection(
          authenticatorAttachment: map['authenticatorSelection']['authenticatorAttachment'],
          residentKey: map['authenticatorSelection']['residentKey'],
          requireResidentKey: map['authenticatorSelection']['requireResidentKey'],
          userVerification: map['authenticatorSelection']['userVerification'],
        ),
        excludeCredentials: (map['excludeCredentials'] as List<dynamic>)
            .map(
              (submap) => PublicKeyCredentialDescriptor(
                type: submap['type'],
                id: submap['id'],
                transports: submap.containsKey('transports') ? submap['transports'] : [],
              ),
            )
            .toList(),
        // extensions: Extensions(
        //   prf: Prf(
        //     eval: Eval(
        //       first: map['extensions']['prf']['eval']['first'],
        //     ),
        //   ),
        // ),
      );
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<AuthRvm> signUp(
    String email,
    String confirmationCode,
    String signatureOverCode,
    RawAttestation attestation,
    String keyShare,
  ) async {
    try {
      var response = await _dio.post(
        '/user/sign-up',
        data: SignUpCommand(
          email: email,
          confirmationCode: confirmationCode,
          signatureOverCode: signatureOverCode,
          rawAttestation: attestation,
          keyShare: keyShare,
        ).toJson(),
      );

      return AuthRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<AssertionOptions> generateAssertionOptions() async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/user/generate-assertion-options',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: {},
      );

      var map = response.data['data'];

      return AssertionOptions(
        rpId: map['rpId'],
        challenge: map['challenge'],
        allowCredentials: (map['allowCredentials'] as List<dynamic>)
            .map(
              (submap) => PublicKeyCredentialDescriptor(
                type: submap['type'],
                id: submap['id'],
                transports: submap.containsKey('transports') ? submap['transports'] : [],
              ),
            )
            .toList(),
        userVerification: map['userVerification'],
        timeout: map['timeout'],
      );
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<String> verifyAssertionAndGetKeyShare(RawAssertion assertion) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/user/verify-assertion-and-get-key-share',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: VerifyAssertionAndGetKeyShareCommand(rawAssertion: assertion).toJson(),
      );

      return response.data['data'] as String;
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<AssertionOptions> generateAssertionOptionsForSignIn() async {
    try {
      var response = await _dio.post(
        '/user/generate-assertion-options-for-sign-in',
        data: {},
      );

      var map = response.data['data'];

      assert((map['allowCredentials'] as List<dynamic>).isEmpty);

      return AssertionOptions(
        rpId: map['rpId'],
        challenge: map['challenge'],
        allowCredentials: (map['allowCredentials'] as List<dynamic>)
            .map(
              (submap) => PublicKeyCredentialDescriptor(
                type: submap['type'],
                id: submap['id'],
                transports: submap.containsKey('transports') ? submap['transports'] : [],
              ),
            )
            .toList(),
        userVerification: map['userVerification'],
        timeout: map['timeout'],
      );
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<AuthRvm> verifyAssertionAndSignIn(RawAssertion assertion) async {
    try {
      var response = await _dio.post(
        '/user/verify-assertion-and-sign-in',
        data: VerifyAssertionAndSignInCommand(rawAssertion: assertion).toJson(),
      );

      return AuthRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }
}
