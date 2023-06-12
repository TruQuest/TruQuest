@JS()
library app;

import 'dart:convert';
import 'dart:js_util';

import 'package:js/js.dart';

@JS()
@anonymous
class EthereumError {
  external int get code;
  external String get message;
}

@JS()
@anonymous
class EthereumRequestAccountsResult {
  external List<String>? get accounts;
  external EthereumError? get error;
}

@JS('EthereumDart')
class _Ethereum {
  external bool isInstalled();
  external dynamic instance();
  external bool isInitialized();
  external dynamic requestAccounts();
  external void on(String event, Function handler);
}

class Ethereum {
  final _Ethereum _ethereum;

  Ethereum() : _ethereum = _Ethereum();

  bool isInstalled() => _ethereum.isInstalled();
  dynamic instance() => _ethereum.instance();
  bool isInitialized() => _ethereum.isInitialized();

  Future<EthereumRequestAccountsResult> requestAccounts() {
    return promiseToFuture<EthereumRequestAccountsResult>(
      _ethereum.requestAccounts(),
    );
  }

  void onAccountsChanged(void Function(List<String>) handler) {
    _ethereum.on(
      'accountsChanged',
      allowInterop(
        (List<dynamic> accounts) => handler(accounts.cast<String>()),
      ),
    );
  }

  void onChainChanged(void Function(int) handler) {
    _ethereum.on(
      'chainChanged',
      allowInterop(
        (dynamic chainId) => handler(int.parse(chainId)),
      ),
    );
  }
}

@JS('ethers.providers.Provider')
abstract class _Provider {
  external _Provider on(String eventName, Function listener);
  external _Provider off(String eventName, [Function? listener]);
  external _Provider removeAllListeners([String? eventName]);
  external dynamic getBlockNumber();
}

abstract class Provider {
  final _Provider _provider;

  Provider(this._provider);

  void onBlockMined(void Function(int blockNumber) handler) {
    _provider.on('block', allowInterop(handler));
  }

  void off(String eventName, [Function? handler]) => _provider.off(
        eventName,
        handler != null ? allowInterop(handler) : null,
      );

  void removeAllListeners([String? eventName]) =>
      _provider.removeAllListeners(eventName);

  Future<int> getBlockNumber() =>
      promiseToFuture<int>(_provider.getBlockNumber());
}

@JS('ethers.providers.Web3Provider')
class _Web3Provider extends _Provider {
  external _Web3Provider(dynamic ethereum);
  external Signer getSigner();
}

class Web3Provider extends Provider {
  late final _Web3Provider _provider;

  Web3Provider(dynamic ethereum) : super(_Web3Provider(ethereum)) {
    _provider = super._provider as _Web3Provider;
  }

  Signer getSigner() => _provider.getSigner();
}

@JS('ethers.providers.JsonRpcProvider')
class _JsonRpcProvider extends _Provider {
  external _JsonRpcProvider([String? rpcUrl]);
}

class JsonRpcProvider extends Provider {
  late final _JsonRpcProvider _provider;

  JsonRpcProvider([String? rpcUrl]) : super(_JsonRpcProvider(rpcUrl)) {
    _provider = super._provider as _JsonRpcProvider;
  }
}

@JS('ethers.Signer')
class Signer {}

class ContractRequestError {
  final int code;
  final String message;

  ContractRequestError({required this.code, required this.message});
}

class ContractExecError {
  final String code;
  final String reason;

  ContractExecError({required this.code, required this.reason});
}

@JS('ethers.Contract')
class _Contract {
  external _Contract(String address, dynamic abi, dynamic providerOrSigner);
  external _Contract connect(dynamic providerOrSigner);
}

class Contract {
  late final _Contract _contract;

  Contract(String address, dynamic abi, dynamic providerOrSigner) {
    if (providerOrSigner is Provider) {
      _contract = _Contract(address, abi, providerOrSigner._provider);
    } else {
      _contract = _Contract(address, abi, providerOrSigner);
    }
  }

  Contract._(this._contract);

  Contract connect(Signer signer) => Contract._(_contract.connect(signer));

  Future<T> get<T>(
    String functionName, {
    List<dynamic> args = const [],
  }) async {
    dynamic jsResult;
    try {
      jsResult = await promiseToFuture(
        callMethod(
          _contract,
          functionName,
          args.map((arg) {
            if (arg is List) {
              return arg
                  .map(
                    (innerArg) =>
                        innerArg is BigInt ? innerArg.toString() : innerArg,
                  )
                  .toList();
            } else if (arg is BigInt) {
              return arg.toString();
            }

            return arg;
          }).toList(),
        ),
      );
    } catch (jsError) {
      if (jsError.runtimeType.toString() == 'JSNoSuchMethodError') {
        throw ContractRequestError(
          code: -51234,
          message: 'Invalid function name',
        );
      } else if (jsError.runtimeType.toString() == 'NativeError') {
        var error = _dartify(jsError);
        throw ContractExecError(
          code: error['code'],
          reason: error['reason'],
        );
      }

      rethrow;
    }

    var result = _dartify(jsResult);
    switch (T) {
      case BigInt:
        var value = result['hex'] as String;
        return BigInt.parse(
          value.startsWith('-') ? '-${value.substring(3)}' : value.substring(2),
          radix: 16,
        ) as T;
      case List:
        var resultList = <dynamic>[];
        for (var item in result) {
          if (item is Map) {
            var value = item['hex'] as String;
            resultList.add(
              BigInt.parse(
                value.startsWith('-')
                    ? '-${value.substring(3)}'
                    : value.substring(2),
                radix: 16,
              ),
            );
          } else {
            resultList.add(item);
          }
        }

        return resultList as T;
      default:
        return result;
    }
  }

  Future<TransactionResponse> post(
    String functionName, {
    List<dynamic> args = const [],
    TransactionOverride? override,
  }) async {
    if (override != null) {
      args = [...args, override];
    }

    try {
      var response = await promiseToFuture<_TransactionResponse>(
        callMethod(
          _contract,
          functionName,
          args.map((arg) {
            if (arg is List) {
              return arg
                  .map(
                    (innerArg) =>
                        innerArg is BigInt ? innerArg.toString() : innerArg,
                  )
                  .toList();
            } else if (arg is BigInt) {
              return arg.toString();
            }

            return arg;
          }).toList(),
        ),
      );

      return TransactionResponse._(response);
    } catch (jsError) {
      var error = _dartify(jsError);
      throw ContractRequestError(
        code: error['code'],
        message: error['message'],
      );
    }
  }
}

@JS()
@anonymous
class TransactionOverride {
  external factory TransactionOverride({
    int? gasLimit,
  });

  external int? get gasLimit;
}

@JS('ethers.BigNumber')
class BigNumber {
  external static BigNumber from(String num);
  external String toHexString();
  external num toNumber();
  @override
  external String toString();
}

@JS()
@anonymous
class _Transaction {
  external String get hash;
  external String get to;
  external String get from;
  external int get nonce;
  external BigNumber get gasLimit;
  external BigNumber get maxFeePerGas;
  external BigNumber get maxPriorityFeePerGas;
  external String get data;
  external BigNumber get value;
  external int get chainId;
  external String get r;
  external String get s;
  external int get v;
}

@JS()
@anonymous
class _TransactionResponse extends _Transaction {
  external int? get blockNumber;
  external String? get blockHash;
  external int? get timestamp;
  external int get confirmations;
  external String? get raw;
  external int get type;

  external dynamic wait([int? confirms]);
}

class TransactionResponse {
  final _TransactionResponse _transactionResponse;

  TransactionResponse._(this._transactionResponse);

  Future<TransactionReceipt> wait([int? confirms]) async {
    try {
      return await promiseToFuture<TransactionReceipt>(
        _transactionResponse.wait(confirms),
      );
    } catch (jsError) {
      var error = _dartify(jsError);
      throw ContractExecError(
        code: error['code'],
        reason: error['reason'],
      );
    }
  }
}

@JS()
@anonymous
class TransactionReceipt {
  external String? get to;
  external String get from;
  external String? get contractAddress;
  external int get transactionIndex;
  external int get type;
  external BigNumber get gasUsed;
  external BigNumber get effectiveGasPrice;
  external String get logsBloom;
  external String get blockHash;
  external String get transactionHash;
  external List<dynamic> get logs;
  external int get blockNumber;
  external int get confirmations;
  external BigNumber get cumulativeGasUsed;
  external bool get byzantium;
  external int get status;
}

@JS('JSON.stringify')
external String _stringify(dynamic obj);

dynamic _dartify(dynamic jsObject) => json.decode(_stringify(jsObject));
