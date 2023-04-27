import 'package:dio/dio.dart';

import '../models/im/sign_in_command.dart';
import '../models/rvm/get_sign_in_data_rvm.dart';
import '../../general/errors/error.dart';
import '../models/rvm/sign_in_rvm.dart';
import '../models/rvm/sign_up_rvm.dart';
import '../../general/services/server_connector.dart';
import '../errors/user_error.dart';
import '../models/im/sign_up_command.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../models/im/sign_up_im.dart';

class UserApiService {
  final Dio _dio;

  UserApiService(ServerConnector serverConnector) : _dio = serverConnector.dio;

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
              return ValidationError();
            } else if (error['type'] == 'User') {
              return UserError(error['errors'].values.first.first);
            }
            break;
          case 401:
            var errorMessage =
                dioError.response!.data['error']['errors'].values.first.first;
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

  Future<SignUpRvm> signUp(String username, String signature) async {
    try {
      var response = await _dio.post(
        '/user/sign-up',
        data: SignUpCommand(
          input: SignUpIm(
            username: username,
          ),
          signature: signature,
        ).toJson(),
      );

      return SignUpRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetSignInDataRvm> getSignInData() async {
    try {
      var response = await _dio.get('/user/sign-in');
      return GetSignInDataRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<SignInRvm> signIn(
    String timestamp,
    String orchestratorSignature,
    String signature,
  ) async {
    try {
      var response = await _dio.post(
        '/user/sign-in',
        data: SignInCommand(
          timestamp: timestamp,
          orchestratorSignature: orchestratorSignature,
          signature: signature,
        ).toJson(),
      );

      return SignInRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
