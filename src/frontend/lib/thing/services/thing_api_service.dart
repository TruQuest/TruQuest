import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:tuple/tuple.dart';

import '../models/rvm/get_thing_result_vm.dart';
import '../models/im/submit_new_thing_command.dart';
import '../models/rvm/submit_new_thing_result_vm.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/file_error.dart';
import '../../general/models/im/tag_im.dart';
import '../models/im/evidence_im.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/error.dart';
import '../../general/services/server_connector.dart';
import '../models/im/new_thing_im.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../errors/thing_error.dart';

class ThingApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  final Map<String, StreamController<int>> _thingIdToProgressChannel = {};

  final StreamController<String> _thingEventChannel =
      StreamController<String>();
  Stream<String> get thingEvent$ => _thingEventChannel.stream;

  ThingApiService(this._serverConnector) : _dio = _serverConnector.dio {
    _serverConnector.serverEvent$
        .where((event) => event.item1 == ServerEventType.thing)
        .listen(
      (event) {
        var data = event.item2 as Tuple2<String, int>;
        var thingId = data.item1;
        var percent = data.item2;
        if (_thingIdToProgressChannel.containsKey(thingId)) {
          _thingIdToProgressChannel[thingId]!.add(percent);
          if (percent == 100) {
            _thingIdToProgressChannel.remove(thingId)!.close();
            _thingEventChannel.add(thingId);
          }
        }
      },
    );
  }

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
            } else if (error['type'] == 'Thing') {
              return ThingError(error['errors'].values.first.first);
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

  // Error _wrapHubException(Exception ex) {
  //   var errorMessage = ex.toString();
  //   if (errorMessage.contains('[AuthorizationError]')) {
  //     if (errorMessage.contains('Forbidden')) {
  //       return ForbiddenError();
  //       // } else if (errorMessage.contains('token expired at')) {
  //       //   return AuthenticationTokenExpiredError();
  //     } else {
  //       return InvalidAuthenticationTokenError(
  //         errorMessage.split('[AuthorizationError] ').last,
  //       );
  //     }
  //   } else if (errorMessage.contains('[ValidationError]')) {
  //     return ValidationError();
  //   } else if (errorMessage.contains('[ThingError]')) {
  //     return ThingError(errorMessage.split('[ThingError] ').last);
  //   }

  //   print(ex);

  //   return ServerError();
  // }

  Future<Stream<int>> createNewThingDraft(
    String subjectId,
    String title,
    String details,
    String? imageExt,
    Uint8List? imageBytes,
    Uint8List? croppedImageBytes,
    List<String> evidence,
    List<int> tags,
  ) async {
    try {
      var input = NewThingIm(
        subjectId: subjectId,
        title: title,
        details: details,
        imageExt: imageExt,
        imageBytes: imageBytes,
        croppedImageExt: croppedImageBytes != null ? 'png' : null,
        croppedImageBytes: croppedImageBytes,
        evidence: evidence.map((url) => EvidenceIm(url: url)).toList(),
        tags: tags.map((tagId) => TagIm(id: tagId)).toList(),
      );

      var response = await _dio.post(
        '/things/draft',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: input.toFormData(),
      );

      var thingId = response.data['data'] as String;

      var progressChannel = StreamController<int>.broadcast();
      _thingIdToProgressChannel[thingId] = progressChannel;

      return progressChannel.stream;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetThingResultVm> getThing(String thingId) async {
    try {
      var response = await _dio.get(
        '/things/$thingId',
        options: _serverConnector.accessToken != null
            ? Options(
                headers: {
                  'Authorization': 'Bearer ${_serverConnector.accessToken}'
                },
              )
            : null,
      );

      return GetThingResultVm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<SubmitNewThingResultVm> submitNewThing(String thingId) async {
    try {
      var response = await _dio.post(
        '/things/submit',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: SubmitNewThingCommand(thingId: thingId).toJson(),
      );

      return SubmitNewThingResultVm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
