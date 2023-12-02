import 'package:dio/dio.dart';

import '../models/vm/tag_vm.dart';
import 'server_connector.dart';
import '../errors/api_error.dart';
import '../errors/error.dart';
import '../errors/connection_error.dart';
import '../errors/server_error.dart';

class GeneralApiService {
  final Dio _dio;

  GeneralApiService(ServerConnector serverConnector) : _dio = serverConnector.dio;

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

        throw UnimplementedError();
      default:
        print(dioError);
        return ApiError();
    }
  }

  Future<List<TagVm>> getTags() async {
    try {
      var response = await _dio.get('/api/tags');
      return List.unmodifiable(
        (response.data['data'] as List<dynamic>).map((map) => TagVm.fromMap(map)),
      );
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
