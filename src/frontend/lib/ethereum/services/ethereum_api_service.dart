import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../models/im/user_operation.dart';
import '../../general/contracts/erc4337/ientrypoint_contract.dart';

class EthereumApiService {
  final String _entryPointAddress;
  late final Dio _dio;
  late final Dio _dioBundler;

  EthereumApiService(IEntryPointContract entryPointContract)
      : _entryPointAddress = entryPointContract.address {
    _dio = Dio(
      BaseOptions(baseUrl: dotenv.env['ETHEREUM_RPC_URL']!),
    );
    _dioBundler = Dio(
      BaseOptions(baseUrl: dotenv.env['ERC4337_BUNDLER_BASE_URL']!),
    );
  }

  Future<BigInt?> getBaseFee() async {
    try {
      var response = await _dio.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_getBlockByNumber',
          'params': ['latest', false],
          'id': 0,
        },
      );

      var fee = response.data['result']['baseFeePerGas'];
      return BigInt.parse(fee);
    } on DioError catch (error) {
      print(error);
      return null;
    }
  }

  Future<BigInt?> getMaxPriorityFee() async {
    try {
      var response = await _dio.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_maxPriorityFeePerGas',
          'params': [],
          'id': 0,
        },
      );

      var fee = response.data['result'];
      return BigInt.parse(fee);
    } on DioError catch (error) {
      print(error);
      return null;
    }
  }

  Future<String?> getCode(String address) async {
    try {
      var response = await _dio.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_getCode',
          'params': [address, 'latest'],
          'id': 0,
        },
      );

      return response.data['result'] as String;
    } on DioError catch (error) {
      print(error);
      return null;
    }
  }

  Future<Map<String, BigInt>?> estimateUserOperationGas(
    UserOperation userOp,
  ) async {
    try {
      var response = await _dioBundler.post(
        '/rpc',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_estimateUserOperationGas',
          'params': [
            userOp.toJson(),
            _entryPointAddress,
          ],
          'id': 0,
        },
      );

      // @@TODO: Check error, which is reported as 200 JSON.

      var result = response.data['result'];
      return {
        'preVerificationGas': BigInt.parse(result['preVerificationGas']),
        'verificationGasLimit': BigInt.parse(result['verificationGasLimit']),
        'callGasLimit': BigInt.parse(result['callGasLimit']),
      };
    } on DioError catch (error) {
      print(error);
      return null;
    }
  }

  Future<String?> sendUserOperation(UserOperation userOp) async {
    try {
      var response = await _dioBundler.post(
        '/rpc',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_sendUserOperation',
          'params': [
            userOp.toJson(),
            _entryPointAddress,
          ],
          'id': 0,
        },
      );

      return response.data['result'] as String;
    } on DioError catch (error) {
      print(error);
      return null;
    }
  }
}
