import 'package:dio/dio.dart';

import '../../general/services/server_connector.dart';
import '../../general/utils/utils.dart';
import '../models/im/edit_whitelist_command.dart';
import '../models/im/fund_with_eth_and_tru_command.dart';
import '../models/im/give_or_remove_restricted_access_command.dart';
import '../models/im/whitelist_entry_type_im.dart';
import '../models/vm/fund_with_eth_and_tru_rvm.dart';
import '../models/vm/get_contracts_states_rvm.dart';
import '../models/vm/get_user_by_email_rvm.dart';

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

  Future addToWhitelist(WhitelistEntryTypeIm entryType, String entry) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/api/admin/whitelist/add',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: EditWhitelistCommand(
          entryType: entryType,
          entry: entry,
        ).toJson(),
      );
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future removeFromWhitelist(WhitelistEntryTypeIm entryType, String entry) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/api/admin/whitelist/remove',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: EditWhitelistCommand(
          entryType: entryType,
          entry: entry,
        ).toJson(),
      );
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<GetUserByEmailRvm> getUserByEmail(String email) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/api/admin/users/$email',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
      );
      return GetUserByEmailRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<String> giveAccessTo(List<String> addresses) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/admin/access/give',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: GiveOrRemoveRestrictedAccessCommand(addresses: addresses).toJson(),
      );

      return response.data['data'] as String;
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<String> removeAccessFrom(String address) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/admin/access/remove',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: GiveOrRemoveRestrictedAccessCommand(addresses: [address]).toJson(),
      );

      return response.data['data'] as String;
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<FundWithEthAndTruRvm> fundWithEthAndTru(String address, double amountInEth, double amountInTru) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/admin/users/$address/fund',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: FundWithEthAndTruCommand(
          amountInEth: amountInEth,
          amountInTru: amountInTru,
        ).toJson(),
      );

      return FundWithEthAndTruRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }
}
