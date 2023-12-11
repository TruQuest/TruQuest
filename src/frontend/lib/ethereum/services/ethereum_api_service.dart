import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../general/utils/logger.dart';
import '../errors/user_operation_error.dart';
import '../models/vm/get_user_operation_receipt_rvm.dart';
import '../models/im/user_operation.dart';
import '../../general/contracts/erc4337/ientrypoint_contract.dart';

class EthereumApiService {
  final String _environment;
  final String _entryPointAddress;
  late final Dio _dio;
  late final Dio _dioBundler;

  EthereumApiService(IEntryPointContract entryPointContract)
      : _environment = dotenv.env['ENVIRONMENT']!,
        _entryPointAddress = entryPointContract.address {
    _dio = Dio(BaseOptions(baseUrl: dotenv.env['ETHEREUM_RPC_URL']!));
    _dioBundler = Dio(BaseOptions(baseUrl: dotenv.env['ERC4337_BUNDLER_URL']!));
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
    } on DioException catch (ex) {
      logger.error('Error trying to get base fee', ex);
      return null;
    }
  }

  Future<BigInt?> getMaxPriorityFee() async {
    try {
      var response = await _dio.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': _environment == 'Development' || dotenv.env['USING_SIMULATED_BLOCKCHAIN'] == '1'
              ? 'eth_maxPriorityFeePerGas'
              : 'rundler_maxPriorityFeePerGas',
          'params': [],
          'id': 0,
        },
      );

      var fee = response.data['result'];
      return BigInt.parse(fee);
    } on DioException catch (ex) {
      logger.error('Error trying to get max priority fee', ex);
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
    } on DioException catch (ex) {
      logger.error('Error trying to get contract code', ex);
      return null;
    }
  }

  Future<BigInt?> getBalance(String address) async {
    try {
      var response = await _dio.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_getBalance',
          'params': [address, 'latest'],
          'id': 0,
        },
      );

      return BigInt.parse(response.data['result'] as String);
    } on DioException catch (ex) {
      logger.error('Error trying to get Eth balance', ex);
      return null;
    }
  }

  Future<(BigInt, BigInt, BigInt)> estimateUserOperationGas(UserOperation userOp) async {
    try {
      var response = await _dioBundler.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_estimateUserOperationGas',
          'params': [userOp.toJson(), _entryPointAddress],
          'id': 0,
        },
      );

      logger.info('===== estimateUserOperationGas =====\n$response');

      if (response.data.containsKey('error')) throw UserOperationError.fromMap(response.data['error']);

      var result = response.data['result'];
      return (
        BigInt.parse(result['preVerificationGas']),
        BigInt.parse(result['verificationGasLimit']),
        BigInt.parse(result['callGasLimit']),
      );
    } on DioException catch (ex) {
      logger.error('Error trying to estimate user operation gas', ex);
      throw UserOperationError(message: 'Estimate user op gas error: ${ex.message}');
    }
  }

  Future<String> sendUserOperation(UserOperation userOp) async {
    try {
      var response = await _dioBundler.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_sendUserOperation',
          'params': [userOp.toJson(), _entryPointAddress],
          'id': 0,
        },
      );

      logger.info('===== sendUserOperation =====\n$response');

      if (response.data.containsKey('error')) throw UserOperationError.fromMap(response.data['error']);

      return response.data['result'] as String;
    } on DioException catch (ex) {
      logger.error('Error trying to send user operation', ex);
      throw UserOperationError(message: 'Send user op error: ${ex.message}');
    }
  }

  Future<GetUserOperationReceiptRvm?> getUserOperationReceipt(String userOpHash) async {
    try {
      var response = await _dioBundler.post(
        '/',
        data: <String, dynamic>{
          'jsonrpc': '2.0',
          'method': 'eth_getUserOperationReceipt',
          'params': [userOpHash],
          'id': 0,
        },
      );

      if (response.data['result'] == null) return null;

      return GetUserOperationReceiptRvm.fromMap(response.data['result']);
    } on DioException catch (ex) {
      logger.error('Error trying to get user operation receipt', ex);
      return null;
    }
  }
}
