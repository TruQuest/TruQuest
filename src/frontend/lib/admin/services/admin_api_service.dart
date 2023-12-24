import 'package:dio/dio.dart';

import '../../general/services/server_connector.dart';
import '../../general/utils/utils.dart';
import '../models/vm/get_contracts_states_rvm.dart';

class AdminApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  AdminApiService(this._serverConnector) : _dio = _serverConnector.dio;

  Future<GetContractsStatesRvm> getContractsStates() async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/api/admin/contracts-states',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
      );
      return GetContractsStatesRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<String> setWithdrawalsEnabled(bool value) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/admin/withdrawals/$value',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
      );
      return response.data['data'] as String;
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<String> setStopTheWorld(bool value) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/admin/stop-the-world/$value',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
      );
      return response.data['data'] as String;
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }
}
