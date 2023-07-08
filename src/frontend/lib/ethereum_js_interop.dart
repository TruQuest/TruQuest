@JS()
library app;

import 'dart:convert';
import 'dart:js_util';

// ignore: depend_on_referenced_packages
import 'package:js/js.dart';

@JS()
@anonymous
class EthereumWalletError {
  external int get code;
  external String get message;
}

@JS()
@anonymous
class EthereumChainIdResult {
  external String? get chainId;
  external EthereumWalletError? get error;
}

@JS()
@anonymous
class EthereumRequestAccountsResult {
  external List<String>? get accounts;
  external EthereumWalletError? get error;
}

@JS()
@anonymous
class EthereumAccountsResult {
  external List<dynamic>? get accounts;
  external EthereumWalletError? get error;
}

@JS()
@anonymous
class EthereumSwitchChainResult {
  external EthereumWalletError? get error;
}

@JS()
@anonymous
class EthereumChainParams {
  external factory EthereumChainParams({
    String id,
    String name,
    String rpcUrl,
  });

  external String get id;
  external String get name;
  external String get rpcUrl;
}

@JS()
@anonymous
class EthereumWatchTruthserumResult {
  external EthereumWalletError? get error;
}

@JS()
@anonymous
class EthereumSignResult {
  external String? get signature;
  external EthereumWalletError? get error;
}

@JS('EthereumWallet')
class _EthereumWallet {
  external bool isInstalled();
  external int count();
  external void select(String walletName);
  external dynamic instance();
  external String name();
  external bool isInitialized();
  external dynamic getChainId();
  external dynamic requestAccounts();
  external dynamic getAccounts();
  external dynamic switchChain(EthereumChainParams chainParams);
  external dynamic watchTruthserum();
  external dynamic signTypedData(String account, String data);
  external dynamic personalSign(String account, String data);
  external dynamic removeAllListeners([String? event]);
  external void on(String event, Function handler);
}

class EthereumWallet {
  final _EthereumWallet _ethereumWallet;

  EthereumWallet() : _ethereumWallet = _EthereumWallet();

  bool isInstalled() => _ethereumWallet.isInstalled();
  int count() => _ethereumWallet.count();
  void select(String walletName) => _ethereumWallet.select(walletName);
  String name() => _ethereumWallet.name();
  dynamic _instance() => _ethereumWallet.instance();
  bool isInitialized() => _ethereumWallet.isInitialized();

  Future<int> getChainId() async {
    var result = await promiseToFuture<EthereumChainIdResult>(
      _ethereumWallet.getChainId(),
    );
    // when starts with 0x defaults to radix = 16
    return int.parse(result.chainId!);
  }

  Future<EthereumRequestAccountsResult> requestAccounts() =>
      promiseToFuture<EthereumRequestAccountsResult>(
        _ethereumWallet.requestAccounts(),
      );

  Future<List<String>> getAccounts() async {
    var result = await promiseToFuture<EthereumAccountsResult>(
      _ethereumWallet.getAccounts(),
    );
    return result.accounts!.cast<String>();
  }

  Future<EthereumWalletError?> switchChain(
    int chainId,
    String chainName,
    String chainRpcUrl,
  ) async {
    var result = await promiseToFuture<EthereumSwitchChainResult>(
      _ethereumWallet.switchChain(
        EthereumChainParams(
          id: '0x' + chainId.toRadixString(16),
          name: chainName,
          rpcUrl: chainRpcUrl,
        ),
      ),
    );

    return result.error;
  }

  Future<EthereumWalletError?> watchTruthserum() async {
    var result = await promiseToFuture<EthereumWatchTruthserumResult>(
      _ethereumWallet.watchTruthserum(),
    );
    return result.error;
  }

  Future<EthereumSignResult> signTypedData(String account, String data) =>
      promiseToFuture<EthereumSignResult>(
        _ethereumWallet.signTypedData(account, data),
      );

  Future<EthereumSignResult> personalSign(String account, String data) =>
      promiseToFuture<EthereumSignResult>(
        _ethereumWallet.personalSign(account, data),
      );

  void removeAllListeners([String? event]) =>
      _ethereumWallet.removeAllListeners(event);

  void onAccountsChanged(void Function(List<String>) handler) {
    _ethereumWallet.on(
      'accountsChanged',
      allowInterop(
        (List<dynamic> accounts) => handler(accounts.cast<String>()),
      ),
    );
  }

  void onChainChanged(void Function(int) handler) {
    var name = _ethereumWallet.name();
    _ethereumWallet.on(
      'chainChanged',
      allowInterop(
        (dynamic chainId) =>
            name == 'Metamask' ? handler(int.parse(chainId)) : handler(chainId),
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

  // ignore: library_private_types_in_public_api
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
  // ignore: annotate_overrides,overridden_fields
  late final _Web3Provider _provider;

  Web3Provider(EthereumWallet ethereumWallet)
      : super(_Web3Provider(ethereumWallet._instance())) {
    _provider = super._provider as _Web3Provider;
  }

  Signer getSigner() => _provider.getSigner();
}

@JS('ethers.providers.JsonRpcProvider')
class _JsonRpcProvider extends _Provider {
  external _JsonRpcProvider([String? rpcUrl]);
}

class JsonRpcProvider extends Provider {
  // ignore: annotate_overrides,overridden_fields
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

  Future<T> read<T>(
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

  Future<TransactionResponse> write(
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
      if (jsError.runtimeType.toString() == 'JSNoSuchMethodError') {
        throw ContractRequestError(
          code: -51234,
          message: 'Invalid function name',
        );
      }

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

@JS('ethers.utils.getAddress')
external String convertToEip55Address(String address);

@JS('JSON.stringify')
external String _stringify(dynamic obj);

dynamic _dartify(dynamic jsObject) => json.decode(_stringify(jsObject));
