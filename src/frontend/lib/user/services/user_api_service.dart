import 'package:dio/dio.dart';

import '../../general/services/server_connector.dart';
import '../errors/user_error.dart';
import '../models/im/sign_up_command.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/authentication_token_expired_error.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../models/im/sign_up_im.dart';

class UserApiService {
  final Dio _dio;

  UserApiService(ServerConnector serverConnector) : _dio = serverConnector.dio;

  dynamic _wrapError(DioError dioError) {
    // ignore: missing_enum_constant_in_switch
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
            break; // @@NOTE: Should never actually reach here.
          case 401:
            var errorMessage =
                dioError.response!.data['error']['errors'].values.first.first;
            if (errorMessage.contains('token expired at')) {
              return AuthenticationTokenExpiredError();
            }
            return InvalidAuthenticationTokenError(errorMessage);
          case 403:
            return ForbiddenError();
        }
    }

    print(dioError);

    return ApiError();
  }

  Future signUp(String username, String signature) async {
    try {
      await _dio.post(
        '/user/signup',
        data: SignUpCommand(
          input: SignUpIm(
            username: username,
          ),
          signature: signature,
        ).toJson(),
      );
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
