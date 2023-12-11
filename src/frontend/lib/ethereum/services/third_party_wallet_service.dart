import 'dart:async';
import 'dart:convert';

import 'package:convert/convert.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:rxdart/rxdart.dart';
import 'package:universal_html/html.dart' as html;

import '../../general/services/local_storage.dart';
import '../../general/utils/logger.dart';
import '../../user/services/user_api_service.dart';
import '../models/vm/wallet_connect_uri_vm.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../errors/wallet_action_declined_error.dart';
import 'iwallet_service.dart';
import '../../ethereum_js_interop.dart';

class ThirdPartyWalletService implements IWalletService {
  final LocalStorage _localStorage;
  final UserApiService _userApiService;

  void Function()? _onSelectedForOnboarding;
  set onSelectedForOnboarding(void Function() f) => _onSelectedForOnboarding = f;

  bool _walletSetup = false;

  late final String _name;
  @override
  String get name => _name;

  final EthereumWallet _ethereumWallet;
  String? _connectedAccount;

  late final _walletConnectProviderOpts = WalletConnectProviderOpts(
    projectId: dotenv.env['WALLET_CONNECT_PROJECT_ID']!,
    chains: [1],
    showQrModal: false,
    methods: [
      'personal_sign', // Checked in MM, works.
      "wallet_scanQRCode",
    ],
    events: ['accountsChanged'],
  );

  final _walletConnectConnectionOpts = WalletConnectConnectionOpts(
    chains: [1],
  );

  final _connectedAccountChangedEventChannel = BehaviorSubject<String?>();
  @override
  Stream<String?> get currentSignerChanged$ => _connectedAccountChangedEventChannel.stream;

  ThirdPartyWalletService(this._localStorage, this._userApiService) : _ethereumWallet = EthereumWallet();

  Future setup(String walletName) async {
    _name = walletName;
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
    _connectedAccount = accounts.firstOrNull;
    _connectedAccountChangedEventChannel.add(_connectedAccount);

    _walletSetup = true;
  }

  void _onAccountsChanged(List<String> accounts) {
    accounts = accounts.map((a) => convertToEip55Address(a)).toList();
    var connectedAccount = accounts.firstOrNull;
    if (_connectedAccount != connectedAccount) {
      _connectedAccount = connectedAccount;
      _connectedAccountChangedEventChannel.add(_connectedAccount);
    }
  }

  Stream<Object> signIn(String? walletName, MultiStageOperationContext ctx) async* {
    if (!_walletSetup) {
      assert(walletName != null);
      _onSelectedForOnboarding?.call();
      await _localStorage.setString('Wallet', jsonEncode({'name': walletName}));
      await setup(walletName!);
    }

    if (_connectedAccount == null) yield* _connectAccount(ctx);
    if (_connectedAccount != null) yield* _signInWithEthereum(ctx);
  }

  Stream<Object> _connectAccount(MultiStageOperationContext ctx) async* {
    if (name == 'WalletConnect') {
      var uriGenerated = Completer<String>();
      _ethereumWallet.onDisplayUriOnce((uri) {
        logger.info('************* WalletConnect URI: $uri *************');
        uriGenerated.complete(uri);
      });

      await _ethereumWallet.requestAccounts(_walletConnectConnectionOpts);

      var uri = await uriGenerated.future;
      yield WalletConnectUriVm(uri: uri);
      return;
    }

    var result = await _ethereumWallet.requestAccounts();
    if (result.isLeft) {
      var error = result.left;
      logger.info('Request accounts error: [${error.code}] ${error.message}');
      yield const WalletActionDeclinedError();
      return;
    }

    var accounts = result.right!;
    accounts = accounts.map((a) => convertToEip55Address(a)).toList();
    var connectedAccount = accounts.firstOrNull;
    if (_connectedAccount != connectedAccount) {
      // @@NOTE: Do this check since I'm not sure what happens first – '_onAccountsChanged' handler
      // gets invoked or 'requestAccounts' call returns.
      _connectedAccount = connectedAccount;
      _connectedAccountChangedEventChannel.add(_connectedAccount);
    }
  }

  Stream<Object> _signInWithEthereum(MultiStageOperationContext ctx) async* {
    var currentSignerAddress = _connectedAccount!;
    var nonce = await _userApiService.getNonceForSiwe(currentSignerAddress);

    var domain = 'truquest.io';
    var statement = 'I accept the TruQuest Terms of Service: https://truquest.io/tos';
    var uri = 'https://truquest.io/';
    var version = 1;

    var now = DateTime.now().toUtc().toIso8601String();
    int indexOfDot = now.indexOf('.');
    var nowWithoutMicroseconds = now.substring(0, indexOfDot + 4) + 'Z';
    var message = '$domain wants you to sign in with your Ethereum account:\n'
        '$currentSignerAddress\n\n'
        '$statement\n\n'
        'URI: $uri\n'
        'Version: $version\n'
        'Nonce: $nonce\n'
        'Issued At: $nowWithoutMicroseconds';

    String signature;
    try {
      signature = await personalSign(message);
    } on WalletActionDeclinedError catch (error) {
      logger.info(error.message);
      yield error;
      return;
    }

    var result = await _userApiService.signInWithEthereum(
      message,
      signature,
    );

    var wallet = jsonDecode(_localStorage.getString('Wallet')!) as Map<String, dynamic>;
    wallet[currentSignerAddress] = {
      'userId': result.userId,
      'walletAddress': convertToEip55Address(result.walletAddress),
      'token': result.token,
    };

    await _localStorage.setString('Wallet', jsonEncode(wallet));

    _connectedAccountChangedEventChannel.add(_connectedAccount);
  }

  @override
  Future<String> personalSign(String message) async {
    var result = await _ethereumWallet.personalSign(
      _connectedAccount!,
      '0x' + hex.encode(utf8.encode(message)),
    );
    if (result.error != null) {
      logger.info('Personal sign message error: [${result.error!.code}] ${result.error!.message}');
      throw const WalletActionDeclinedError();
    }

    return result.signature!;
  }

  @override
  Future<String> personalSignDigest(String digest) async {
    var result = await _ethereumWallet.personalSign(
      _connectedAccount!,
      digest,
    );
    if (result.error != null) {
      logger.info('Personal sign message error: [${result.error!.code}] ${result.error!.message}');
      throw const WalletActionDeclinedError();
    }

    return result.signature!;
  }
}
