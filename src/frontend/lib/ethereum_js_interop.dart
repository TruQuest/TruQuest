@JS()
library app;

import 'dart:convert';
import 'dart:js_util';
import 'dart:typed_data';

// ignore: depend_on_referenced_packages
import 'package:js/js.dart';
import 'package:convert/convert.dart';

@JS()
@anonymous
class EthereumWalletError {
  external int get code;
  external String get message;
}

@JS()
@anonymous
class EthereumRequestAccountsResult {
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
  external dynamic select(
    String walletName, [
    WalletConnectProviderOpts? walletConnectProviderOpts,
  ]);
  external bool isInitialized();
  external dynamic requestAccounts([
    WalletConnectConnectionOpts? walletConnectConnectionOpts,
  ]);
  external dynamic getAccounts();
  external dynamic watchTruthserum();
  external dynamic personalSign(String account, String data);
  external void removeListener(String event, Function handler);
  external void on(String event, Function handler);
  external void once(String event, Function handler);
}

class EthereumWallet {
  final _EthereumWallet _ethereumWallet;

  EthereumWallet() : _ethereumWallet = _EthereumWallet();

  Future select(
    String walletName, [
    WalletConnectProviderOpts? walletConnectProviderOpts,
  ]) =>
      promiseToFuture(
        _ethereumWallet.select(walletName, walletConnectProviderOpts),
      );

  bool isInitialized() => _ethereumWallet.isInitialized();

  Future<EthereumWalletError?> requestAccounts([
    WalletConnectConnectionOpts? walletConnectConnectionOpts,
  ]) async {
    var result = await promiseToFuture<EthereumRequestAccountsResult>(
      _ethereumWallet.requestAccounts(walletConnectConnectionOpts),
    );
    return result.error;
  }

  Future<List<String>> getAccounts() async {
    var result = await promiseToFuture<EthereumAccountsResult>(
      _ethereumWallet.getAccounts(),
    );
    return result.accounts!.cast<String>();
  }

  Future<EthereumWalletError?> watchTruthserum() async {
    var result = await promiseToFuture<EthereumWatchTruthserumResult>(
      _ethereumWallet.watchTruthserum(),
    );
    return result.error;
  }

  Future<EthereumSignResult> personalSign(String account, String data) =>
      promiseToFuture<EthereumSignResult>(
        _ethereumWallet.personalSign(account, data),
      );

  void removeListener(String event, Function handler) =>
      _ethereumWallet.removeListener(event, allowInterop(handler));

  void onDisplayUriOnce(void Function(String) handler) {
    _ethereumWallet.once(
      'display_uri',
      allowInterop(
        (dynamic uri) => handler(uri as String),
      ),
    );
  }

  void onAccountsChanged(void Function(List<String>) handler) {
    _ethereumWallet.on(
      'accountsChanged',
      allowInterop(
        (List<dynamic> accounts) => handler(accounts.cast<String>()),
      ),
    );
  }

  void onChainChanged(void Function(int) handler) {
    _ethereumWallet.on(
      'chainChanged',
      allowInterop(
        (dynamic chainId) =>
            chainId is String ? handler(int.parse(chainId)) : handler(chainId),
      ),
    );
  }
}

@JS('ethers.providers.Provider')
abstract class _Provider {
  external _Provider on(String eventName, Function listener);
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

  void removeAllListeners([String? eventName]) =>
      _provider.removeAllListeners(eventName);

  Future<int> getBlockNumber() =>
      promiseToFuture<int>(_provider.getBlockNumber());
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
class _Signer {
  external dynamic getAddress();
}

class Signer {
  final _Signer _signer;

  // ignore: library_private_types_in_public_api
  Signer(this._signer);

  Future<String> getAddress() => promiseToFuture<String>(_signer.getAddress());
}

@JS('ethers.utils.Interface')
class _Interface {
  external _Interface(dynamic abi);
  external String encodeFunctionData(String fragment, [dynamic values]);
}

class Abi {
  final _Interface _interface;

  Abi(dynamic abi) : _interface = _Interface(abi);

  String encodeFunctionData(String fragment, [dynamic values]) =>
      _interface.encodeFunctionData(fragment, values);
}

@JS('ethers.Wallet.createRandom')
external _Wallet _createRandomWallet();

@JS('ethers.Wallet')
class _Wallet {
  external _Mnemonic get mnemonic;
}

class Wallet {
  final _Wallet _wallet;

  String get mnemonic => _wallet.mnemonic.phrase;

  Wallet._(this._wallet);

  static Wallet createRandom() => Wallet._(_createRandomWallet());
}

@JS('ethers.utils.HDNode.fromMnemonic')
external _HDNode _createHDNodeFromMnemonic(String phrase, String password);

@JS('ethers.utils.HDNode')
class _HDNode {
  external String get address;
  external String privateKey;
  external _Mnemonic get mnemonic;

  external _HDNode derivePath(String path);
}

class HDNode {
  final _HDNode _hdNode;

  HDNode._(this._hdNode);

  static HDNode fromMnemonic(String phrase, String password) =>
      HDNode._(_createHDNodeFromMnemonic(phrase, password));

  String get address => convertToEip55Address(_hdNode.address);
  String get privateKey => _hdNode.privateKey;
  String get mnemonic => _hdNode.mnemonic.phrase;

  HDNode derivePath(String path) => HDNode._(_hdNode.derivePath(path));
}

@JS('ethers.Mnemonic')
class _Mnemonic {
  external String get phrase;
}

@JS('ethers.utils.keccak256')
external String keccak256(Uint8List data);

@JS('ethers.utils.hashMessage')
external String hashMessage(Uint8List message);

@JS('ethers.utils.SigningKey')
class SigningKey {
  external SigningKey(Uint8List privateKey);
  external Signature signDigest(Uint8List digest);
}

@JS('ethers.crypto.Signature')
class Signature {
  external String get r;
  external String get s;
  external int get v;
}

extension SignatureExtension on Signature {
  String get combined =>
      '0x' + r.substring(2) + s.substring(2) + hex.encode([v]);
}

@JS('ethers.utils.formatUnits')
external String formatUnits(BigNumber value, [String unit = 'ether']);

@JS('ethers.Contract')
class _Contract {
  external _Contract(String address, dynamic abi, dynamic providerOrSigner);
  external _Contract connect(dynamic providerOrSigner);
}

class Contract {
  late final _Contract _contract;

  Contract(String address, dynamic abi, dynamic providerOrSigner) {
    assert(providerOrSigner is Provider || providerOrSigner is Signer);
    if (providerOrSigner is Provider) {
      _contract = _Contract(address, abi, providerOrSigner._provider);
    } else {
      _contract = _Contract(address, abi, providerOrSigner._signer);
    }
  }

  Contract._(this._contract);

  Contract connect(Signer signer) =>
      Contract._(_contract.connect(signer._signer));

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
      print('Contract.read JS error: $jsError');
      dynamic error;
      try {
        error = _dartify(jsError);
        print('Contract.read error: $error');
      } catch (_) {
        error = 'Contract.read error';
      }

      throw error;
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
      print('Contract.write JS error: $jsError');
      dynamic error;
      try {
        error = _dartify(jsError);
        print('Contract.write error: $error');
      } catch (_) {
        error = 'Contract.write error';
      }

      throw error;
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
      print('TransactionResponse.wait JS error: $jsError');
      dynamic error;
      try {
        error = _dartify(jsError);
        print('TransactionResponse.wait error: $error');
      } catch (_) {
        error = 'TransactionResponse.wait error';
      }

      throw error;
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

@JS()
@anonymous
class Redirect {
  external factory Redirect({
    String? native,
    String? universal,
  });

  external String? get native;
  external String? get universal;
}

@JS()
@anonymous
class Metadata {
  external factory Metadata({
    String name,
    String description,
    String url,
    List<String> icons,
    String? verifyUrl,
    Redirect? redirect,
  });

  external String get name;
  external String get description;
  external String get url;
  external List<String> get icons;
  external String? get verifyUrl;
  external Redirect? get redirect;
}

@JS()
@anonymous
class WalletConnectProviderOpts {
  external factory WalletConnectProviderOpts({
    String projectId,
    List<int> chains,
    List<int>? optionalChains,
    List<String>? methods,
    List<String>? optionalMethods,
    List<String>? events,
    List<String>? optionalEvents,
    dynamic rpcMap,
    Metadata? metadata,
    bool showQrModal,
  });

  external String get projectId;
  external List<int> get chains;
  external List<int>? get optionalChains;
  external List<String>? get methods;
  external List<String>? get optionalMethods;
  external List<String>? get events;
  external List<String>? get optionalEvents;
  external dynamic get rpcMap;
  external Metadata? get metadata;
  external bool get showQrModal;
}

@JS()
@anonymous
class WalletConnectConnectionOpts {
  external factory WalletConnectConnectionOpts({
    List<int>? chains,
    List<int>? optionalChains,
    String? pairingTopic,
  });

  external List<int>? get chains;
  external List<int>? get optionalChains;
  external String? get pairingTopic;
}
