import 'package:dio/dio.dart';

import '../models/vm/get_contracts_states_rvm.dart';
import '../models/vm/tag_vm.dart';
import 'server_connector.dart';
import '../utils/utils.dart';

class GeneralApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  GeneralApiService(this._serverConnector) : _dio = _serverConnector.dio;

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

  Future<GetContractsStatesRvm> getContractsStates() async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/api/contracts-states',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
      );
      return GetContractsStatesRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }
}
