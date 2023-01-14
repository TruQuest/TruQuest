import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../models/im/new_subject_im.dart';
import '../../general/errors/file_error.dart';
import '../../general/models/im/tag_im.dart';
import '../errors/subject_error.dart';
import '../models/im/subject_type_im.dart';
import '../../general/errors/error.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../../general/services/server_connector.dart';

class SubjectApiService {
  final Dio _dio;

  SubjectApiService(ServerConnector serverConnector)
      : _dio = serverConnector.dio;

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
            } else if (error['type'] == 'File') {
              return FileError(error['errors'].values.first.first);
            } else if (error['type'] == 'Subject') {
              return SubjectError(error['errors'].values.first.first);
            }
            break; // @@NOTE: Should never actually reach here.
          case 401:
            var errorMessage =
                dioError.response!.data['error']['errors'].values.first.first;
            return InvalidAuthenticationTokenError(errorMessage);
          case 403:
            return ForbiddenError();
        }
    }

    print(dioError);

    return ApiError();
  }

  Future<String> addNewSubject(
    SubjectTypeIm type,
    String name,
    String details,
    String imageExt,
    Uint8List imageBytes,
    Uint8List croppedImageBytes,
    List<int> tags,
  ) async {
    try {
      var input = NewSubjectIm(
        type: type,
        name: name,
        details: details,
        imageExt: imageExt,
        imageBytes: imageBytes,
        croppedImageExt: 'png',
        croppedImageBytes: croppedImageBytes,
        tags: tags.map((tagId) => TagIm(id: tagId)).toList(),
      );

      var response = await _dio.post(
        '/subjects/add',
        // options: Options(
        //   headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        // ),
        data: input.toFormData(),
      );

      return response.data['data'] as String;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
