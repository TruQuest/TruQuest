import 'dart:async';
import 'dart:convert';

import 'package:convert/convert.dart';
import 'package:either_dart/either.dart';
import 'package:universal_html/html.dart' as html;

import '../../general/services/local_storage.dart';
import '../../ethereum_js_interop.dart';
import '../../thing/models/im/decision_im.dart' as thing;
import '../../settlement/models/im/decision_im.dart' as settlement;
import '../errors/ethereum_error.dart';

class EthereumService {
  final LocalStorage _localStorage;

  final int validChainId = 901;
  final String validChainName = 'Optimism Local';

  int? _connectedChainId;
  int? get connectedChainId => _connectedChainId;

  String? _connectedAccount;
  String? get connectedAccount => _connectedAccount;

  final EthereumWallet _ethereumWallet;
  // late final Web3Provider provider;
  late final JsonRpcProvider l2ReadOnlyProvider;
  late final JsonRpcProvider l1Provider;

  final walletSetup = Completer<String>();
  final _walletConnectConnectionOpts = WalletConnectConnectionOpts(
    chains: [1],
    optionalChains: [901],
  );

  final _connectedChainChangedEventChannel = StreamController<(int, bool)>();
  Stream<(int, bool)> get connectedChainChanged$ =>
      _connectedChainChangedEventChannel.stream;

  final _connectedAccountChangedEventChannel = StreamController<String?>();
  Stream<String?> get connectedAccountChanged$ =>
      _connectedAccountChangedEventChannel.stream;

  final _l1BlockMinedEventChannel = StreamController<int>();
  Stream<int> get l1BlockMined$ => _l1BlockMinedEventChannel.stream;

  EthereumService(this._localStorage) : _ethereumWallet = EthereumWallet() {
    l1Provider = JsonRpcProvider('http://localhost:8545');
    l1Provider.removeAllListeners('block');
    l1Provider.onBlockMined((blockNumber) {
      print('Latest L1 block: $blockNumber');
      _l1BlockMinedEventChannel.add(blockNumber);
    });

    l2ReadOnlyProvider = JsonRpcProvider('http://localhost:9545');

    String? walletName;
    if ((walletName = _localStorage.getString('SelectedWallet')) != null) {
      _ethereumWallet.select(walletName!);
      _setup(walletName);
    }
  }

  void _setup(String walletName) {
    if (!_ethereumWallet.isInitialized()) {
      html.window.location.reload();
    }

    // provider = Web3Provider(_ethereumWallet);

    _ethereumWallet.removeListener('chainChanged', _onChainChanged);
    _ethereumWallet.removeListener('accountsChanged', _onAccountsChanged);

    _ethereumWallet.onChainChanged(_onChainChanged);
    _ethereumWallet.onAccountsChanged(_onAccountsChanged);

    // if (walletName != 'WalletConnect' ||
    //     _ethereumWallet.walletConnectSessionExists()) {
    //   _ethereumWallet.getChainId().then((chainId) {
    //     print('Current chain: $chainId');
    //     int? oldChainId = _connectedChainId;
    //     _connectedChainId = chainId;
    //     _connectedChainChangedEventChannel.add(
    //       (_connectedChainId!, oldChainId != null),
    //     );
    //   });

    //   _ethereumWallet.getAccounts().then((accounts) {
    //     accounts = accounts.map((a) => convertToEip55Address(a)).toList();
    //     _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
    //     print('Current account: $_connectedAccount');
    //     _connectedAccountChangedEventChannel.add(_connectedAccount);
    //   });
    // }

    walletSetup.complete(walletName);
  }

  void _onChainChanged(int chainId) {
    if (_connectedChainId != chainId) {
      print('Chain changed: $chainId');
      int? oldChainId = _connectedChainId;
      _connectedChainId = chainId;
      _connectedChainChangedEventChannel.add(
        (_connectedChainId!, oldChainId != null),
      );
    }
  }

  void _onAccountsChanged(List<String> accounts) {
    accounts = accounts.map((a) => convertToEip55Address(a)).toList();
    var connectedAccount = accounts.isNotEmpty ? accounts.first : null;
    if (_connectedAccount != connectedAccount) {
      print('Accounts changed: $accounts');
      _connectedAccount = connectedAccount;
      _connectedAccountChangedEventChannel.add(_connectedAccount);
    }
  }

  Future<int> getLatestL1BlockNumber() => l1Provider.getBlockNumber();

  Future selectWallet(String walletName) async {
    assert(walletName == 'Metamask' ||
        walletName == 'CoinbaseWallet' ||
        walletName == 'WalletConnect');

    // @@TODO: Try-catch selected wallet not available.
    _ethereumWallet.select(walletName);
    await _localStorage.setString('SelectedWallet', walletName);
    _setup(walletName);
  }

  Future<EthereumError?> switchEthereumChain() async {
    if (!walletSetup.isCompleted) {
      return EthereumError('Wallet not selected');
    }

    // var error = await _ethereumWallet.switchChain(
    //   validChainId,
    //   'Optimism Local',
    //   'https://3c39-49-166-47-41.ngrok.io',
    // );
    // if (error != null) {
    //   print('Switch chain error: [${error.code}] ${error.message}');
    //   return EthereumError('Error trying to switch chain');
    // }

    return null;
  }

  Future<Either<EthereumError, String?>> connectAccount() async {
    if (!walletSetup.isCompleted) {
      return Left(EthereumError('Wallet not selected'));
    }

    var walletName = await walletSetup.future;
    if (walletName == 'WalletConnect') {
      var uriGenerated = Completer<String>();
      _ethereumWallet.onDisplayUriOnce((uri) {
        print('WalletConnect URI: $uri');
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

  Future<Either<EthereumError, String>> signThingAcceptancePollVote(
    String thingId,
    String castedAt,
    thing.DecisionIm decision,
    String reason,
  ) async {
    if (!walletSetup.isCompleted) {
      return Left(EthereumError('Wallet not selected'));
    }

    var connectedAccount = _connectedAccount;
    if (connectedAccount == null) {
      return Left(EthereumError('No account connected'));
    }

    Map<String, dynamic> map = {
      'types': {
        'EIP712Domain': [
          {'name': 'name', 'type': 'string'},
          {'name': 'version', 'type': 'string'},
          {'name': 'chainId', 'type': 'uint256'},
          {'name': 'verifyingContract', 'type': 'address'},
          {'name': 'salt', 'type': 'bytes32'},
        ],
        'NewAcceptancePollVoteTd': [
          {'name': 'thingId', 'type': 'string'},
          {'name': 'castedAt', 'type': 'string'},
          {'name': 'decision', 'type': 'string'},
          {'name': 'reason', 'type': 'string'},
        ],
      },
      'domain': {
        'name': 'TruQuest',
        'version': '0.0.1',
        'chainId': validChainId,
        'verifyingContract': '0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c',
        'salt':
            '0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558',
      },
      'primaryType': 'NewAcceptancePollVoteTd',
      'message': {
        'thingId': thingId,
        'castedAt': castedAt,
        'decision': decision.getString(),
        'reason': reason,
      },
    };

    var data = jsonEncode(map);

    // var result = await _ethereumWallet.signTypedData(connectedAccount, data);
    // if (result.error != null) {
    //   print(
    //     'Sign message error: [${result.error!.code}] ${result.error!.message}',
    //   );
    //   return Left(EthereumError('Error signing message'));
    // }

    return Right('');
  }

  Future<Either<EthereumError, String>>
      signThingSettlementProposalAssessmentPollVote(
    String thingId,
    String proposalId,
    String castedAt,
    settlement.DecisionIm decision,
    String reason,
  ) async {
    if (!walletSetup.isCompleted) {
      return Left(EthereumError('Wallet not selected'));
    }

    var connectedAccount = _connectedAccount;
    if (connectedAccount == null) {
      return Left(EthereumError('No account connected'));
    }

    Map<String, dynamic> map = {
      'types': {
        'EIP712Domain': [
          {'name': 'name', 'type': 'string'},
          {'name': 'version', 'type': 'string'},
          {'name': 'chainId', 'type': 'uint256'},
          {'name': 'verifyingContract', 'type': 'address'},
          {'name': 'salt', 'type': 'bytes32'},
        ],
        'NewAssessmentPollVoteTd': [
          {'name': 'thingId', 'type': 'string'},
          {'name': 'settlementProposalId', 'type': 'string'},
          {'name': 'castedAt', 'type': 'string'},
          {'name': 'decision', 'type': 'string'},
          {'name': 'reason', 'type': 'string'},
        ],
      },
      'domain': {
        'name': 'TruQuest',
        'version': '0.0.1',
        'chainId': validChainId,
        'verifyingContract': '0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c',
        'salt':
            '0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558',
      },
      'primaryType': 'NewAssessmentPollVoteTd',
      'message': {
        'thingId': thingId,
        'settlementProposalId': proposalId,
        'castedAt': castedAt,
        'decision': decision.getString(),
        'reason': reason,
      },
    };

    var data = jsonEncode(map);

    // var result = await _ethereumWallet.signTypedData(connectedAccount, data);
    // if (result.error != null) {
    //   print(
    //     'Sign message error: [${result.error!.code}] ${result.error!.message}',
    //   );
    //   return Left(EthereumError('Error signing message'));
    // }

    return Right('');
  }

  Future<Either<EthereumError, (String, String)>> signSiweMessage(
    String account,
    String nonce,
  ) async {
    if (!walletSetup.isCompleted) {
      return Left(EthereumError('Wallet not selected'));
    }

    var connectedAccount = _connectedAccount;
    if (connectedAccount == null) {
      return Left(EthereumError('No account connected'));
    }

    if (connectedAccount != account) {
      return Left(EthereumError("Requested nonce is for another account"));
    }

    var domain = 'truquest.io';
    var statement =
        'I accept the TruQuest Terms of Service: https://truquest.io/tos';
    var uri = 'https://truquest.io/sign-in';
    var version = 1;

    var now = DateTime.now().toUtc().toIso8601String();
    int indexOfDot = now.indexOf('.');
    var nowWithoutMicroseconds = now.substring(0, indexOfDot + 4) + 'Z';
    var message = '$domain wants you to sign in with your Ethereum account:\n'
        '$account\n\n'
        '$statement\n\n'
        'URI: $uri\n'
        'Version: $version\n'
        'Chain ID: $_connectedChainId\n'
        'Nonce: $nonce\n'
        'Issued At: $nowWithoutMicroseconds';

    var result = await _ethereumWallet.personalSign(
      connectedAccount,
      '0x' + hex.encode(utf8.encode(message)),
    );
    if (result.error != null) {
      print(
        'Personal sign message error: [${result.error!.code}] ${result.error!.message}',
      );
      return Left(EthereumError('Error personal signing message'));
    }

    return Right((message, result.signature!));
  }
}
