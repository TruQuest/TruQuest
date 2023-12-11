import 'package:dio/dio.dart';

import '../models/vm/tag_vm.dart';
import 'server_connector.dart';
import '../utils/utils.dart';

class GeneralApiService {
  final Dio _dio;

  GeneralApiService(ServerConnector serverConnector) : _dio = serverConnector.dio;

  Future<List<TagVm>> getTags() async {
    try {
      var response = await _dio.get('/api/tags');
      return List.unmodifiable(
        (response.data['data'] as List<dynamic>).map((map) => TagVm.fromMap(map)),
      );
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }
}
