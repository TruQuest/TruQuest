import 'package:dio/dio.dart';

import '../../ethereum_js_interop.dart';
import '../models/im/add_email_command.dart';
import '../models/im/confirm_email_command.dart';
import '../models/im/generate_assertion_options_command.dart';
import '../models/im/generate_confirmation_code_and_attestation_options_command.dart';
import '../models/im/sign_in_with_ethereum_command.dart';
import '../models/im/sign_up_command.dart';
import '../models/im/verify_assertion_and_get_key_share_command.dart';
import '../models/rvm/sign_in_with_ethereum_rvm.dart';
import '../models/im/mark_notifications_as_read_command.dart';
import '../models/im/notification_im.dart';
import '../models/im/watched_item_type_im.dart';
import '../../general/models/rvm/notification_vm.dart';
import '../../general/errors/error.dart';
import '../../general/services/server_connector.dart';
import '../errors/user_error.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../models/rvm/sign_up_rvm.dart';

class UserApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  UserApiService(this._serverConnector) : _dio = _serverConnector.dio;

  Error _wrapError(DioError dioError) {
    switch (dioError.type) {
      case DioErrorType.connectTimeout:
      case DioErrorType.sendTimeout:
      case DioErrorType.receiveTimeout:
        return ConnectionError();
      case DioErrorType.response:
        var statusCode = dioError.response!.statusCode!;
        if (statusCode >= 500) {
          return ServerError();
        }

        switch (statusCode) {
          case 400:
            var error = dioError.response!.data['error'];
            if (error['type'] == 'Validation') {
              return const ValidationError();
            } else if (error['type'] == 'User') {
              return UserError(error['errors'].values.first.first);
            }
            break;
          case 401:
            var errorMessage = dioError.response!.data['error']['errors'].values.first.first;
            // if (errorMessage.contains('token expired at')) {
            //   return AuthenticationTokenExpiredError();
            // }
            return InvalidAuthenticationTokenError(errorMessage);
          case 403:
            return ForbiddenError();
        }

        throw UnimplementedError();
      default:
        print(dioError);
        return ApiError();
    }
  }

  Future<String> getNonceForSiwe(String account) async {
    try {
      var response = await _dio.get('/user/siwe/$account');
      return response.data['data'] as String;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<SignInWithEthereumRvm> signInWithEthereum(
    String message,
    String signature,
  ) async {
    try {
      var response = await _dio.post(
        '/user/siwe',
        data: SignInWithEthereumCommand(
          message: message,
          signature: signature,
        ).toJson(),
      );

      return SignInWithEthereumRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future addEmail(String email) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/user/email',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: AddEmailCommand(email: email).toJson(),
      );
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future confirmEmail(String confirmationToken) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/user/email/confirm',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: ConfirmEmailCommand(
          confirmationToken: confirmationToken,
        ).toJson(),
      );
    } on DioError catch (error) {
      throw _wrapError(error);
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
      throw _wrapError(error);
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
                transports: submap.containsKey('transports') ? submap['transports'] : null,
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
      throw _wrapError(error);
    }
  }

  Future<SignUpRvm> signUp(
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

      return SignUpRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
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
        data: GenerateAssertionOptionsCommand().toJson(),
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
                transports: submap.containsKey('transports') ? submap['transports'] : null,
              ),
            )
            .toList(),
        userVerification: map['userVerification'],
        timeout: map['timeout'],
      );
    } on DioError catch (error) {
      throw _wrapError(error);
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
      throw _wrapError(error);
    }
  }
}
