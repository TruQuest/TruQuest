import 'dart:async';
import 'dart:convert';

import 'package:convert/convert.dart';
import 'package:either_dart/either.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:rxdart/rxdart.dart';
import 'package:universal_html/html.dart' as html;

import '../../general/contracts/erc4337/iaccount_factory_contract.dart';
import 'iwallet_service.dart';
import '../errors/ethereum_error.dart';
import '../../ethereum_js_interop.dart';

class ThirdPartyWalletService implements IWalletService {
  final IAccountFactoryContract _accountFactoryContract;

  final EthereumWallet _ethereumWallet;
  String? _connectedAccount;

  final walletSetup = Completer<String>();

  late final _walletConnectProviderOpts = WalletConnectProviderOpts(
    projectId: dotenv.env['WALLET_CONNECT_PROJECT_ID']!,
    chains: [1],
    showQrModal: false,
    methods: [
      'personal_sign', // Checked in MM, works.
      'wallet_watchAsset',
      "wallet_scanQRCode",
    ],
    events: ['accountsChanged'],
  );

  final _walletConnectConnectionOpts = WalletConnectConnectionOpts(
    chains: [1],
  );

  final _connectedAccountChangedEventChannel = BehaviorSubject<String?>();

  @override
  Stream<String?> get currentWalletAddressChanged$ =>
      _connectedAccountChangedEventChannel.asyncMap((connectedAccount) async {
        _connectedAccount = connectedAccount;
        _currentWalletAddress = connectedAccount != null
            ? await _accountFactoryContract.getAddress(connectedAccount)
            : null;

        print(
          '*************** $_connectedAccount: $_currentWalletAddress ***************',
        );

        return _currentWalletAddress;
      });

  String? _currentWalletAddress;

  @override
  String? get currentWalletAddress => _currentWalletAddress;

  @override
  String? get currentOwnerAddress => _connectedAccount;

  @override
  bool get isUnlocked => true;

  ThirdPartyWalletService(this._accountFactoryContract)
      : _ethereumWallet = EthereumWallet();

  Future<bool> setup(String walletName) async {
    // @@TODO: Try-catch selected wallet not available.
    await _ethereumWallet.select(
      walletName,
      walletName == 'WalletConnect' ? _walletConnectProviderOpts : null,
    );

    if (!_ethereumWallet.isInitialized()) {
      html.window.location.reload();
    }

    _ethereumWallet.removeListener('accountsChanged', _onAccountsChanged);
    _ethereumWallet.onAccountsChanged(_onAccountsChanged);

    // @@NOTE: WalletConnect: when we request accounts on startup, it always
    // returns the one with which we initially connected, even if we switched
    // to another account in the previous session.
    var accounts = await _ethereumWallet.getAccounts();
    accounts = accounts.map((a) => convertToEip55Address(a)).toList();
    var connectedAccount = accounts.firstOrNull;
    _connectedAccountChangedEventChannel.add(connectedAccount);

    walletSetup.complete(walletName);

    return connectedAccount != null;
  }

  void _onAccountsChanged(List<String> accounts) {
    accounts = accounts.map((a) => convertToEip55Address(a)).toList();
    var connectedAccount = accounts.firstOrNull;
    if (_connectedAccount != connectedAccount) {
      _connectedAccountChangedEventChannel.add(connectedAccount);
    }
  }

  Future<Either<EthereumError, String?>> connectAccount() async {
    var walletName = await walletSetup.future;
    if (walletName == 'WalletConnect') {
      var uriGenerated = Completer<String>();
      _ethereumWallet.onDisplayUriOnce((uri) {
        print('************* WalletConnect URI: $uri *************');
        uriGenerated.complete(uri);
      });

      await _ethereumWallet.requestAccounts(_walletConnectConnectionOpts);

      var uri = await uriGenerated.future;
      return Right(uri);
    }

    var error = await _ethereumWallet.requestAccounts();
    if (error != null) {
      print('Request accounts error: [${error.code}] ${error.message}');
      return Left(EthereumError('Error requesting accounts'));
    }

    return const Right(null);
  }

  void watchTruthserum() async {
    var error = await _ethereumWallet.watchTruthserum();
    if (error != null) {
      print('Watch Truthserum error: [${error.code}] ${error.message}');
    }
  }

  @override
  FutureOr<String> personalSign(String message) async {
    var result = await _ethereumWallet.personalSign(
      _connectedAccount!,
      '0x' + hex.encode(utf8.encode(message)),
    );
    if (result.error != null) {
      print(
        'Personal sign message error: [${result.error!.code}] ${result.error!.message}',
      );
      throw 'Alala';
    }

    return result.signature!;
  }

  @override
  FutureOr<String> personalSignDigest(String digest) async {
    var result = await _ethereumWallet.personalSign(
      _connectedAccount!,
      digest,
    );
    if (result.error != null) {
      print(
        'Personal sign message error: [${result.error!.code}] ${result.error!.message}',
      );
      throw 'Alala';
    }

    return result.signature!;
  }
}
