import 'dart:async';
import 'dart:convert';

import 'package:convert/convert.dart';
import 'package:either_dart/either.dart';
import 'package:universal_html/html.dart' as html;

import '../../ethereum_js_interop.dart';
import '../../thing/models/im/decision_im.dart' as thing;
import '../../settlement/models/im/decision_im.dart' as settlement;
import '../errors/ethereum_error.dart';

class EthereumService {
  final int validChainId = 901;

  int? _connectedChainId;
  int? get connectedChainId => _connectedChainId;

  String? _connectedAccount;
  String? get connectedAccount => _connectedAccount;

  final EthereumWallet _ethereumWallet;
  late final Web3Provider provider;
  late final JsonRpcProvider l1Provider;

  bool get available => _ethereumWallet.isInstalled();

  final StreamController<(int, bool)> _connectedChainChangedEventChannel =
      StreamController<(int, bool)>();
  Stream<(int, bool)> get connectedChainChanged$ =>
      _connectedChainChangedEventChannel.stream;

  final StreamController<String?> _connectedAccountChangedEventChannel =
      StreamController<String?>();
  Stream<String?> get connectedAccountChanged$ =>
      _connectedAccountChangedEventChannel.stream;

  final StreamController<int> _l1BlockMinedEventChannel =
      StreamController<int>();
  Stream<int> get l1BlockMined$ => _l1BlockMinedEventChannel.stream;

  EthereumService() : _ethereumWallet = EthereumWallet() {
    l1Provider = JsonRpcProvider('http://localhost:8545');
    if (available) {
      provider = Web3Provider(_ethereumWallet);

      if (!_ethereumWallet.isInitialized()) {
        html.window.location.reload();
      }

      _ethereumWallet.removeAllListeners('chainChanged');
      _ethereumWallet.removeAllListeners('accountsChanged');
      l1Provider.removeAllListeners('block');

      _ethereumWallet.onChainChanged((chainId) {
        print('Chain changed: $chainId');
        if (_connectedChainId != chainId) {
          _connectedChainId = chainId;
          _connectedChainChangedEventChannel.add(
            (_connectedChainId!, true),
          );
        }
      });

      _ethereumWallet.onAccountsChanged((accounts) {
        accounts = accounts.map((a) => convertToEip55Address(a)).toList();
        print('Accounts changed: $accounts');
        var connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        if (_connectedAccount != connectedAccount) {
          _connectedAccount = connectedAccount;
          _connectedAccountChangedEventChannel.add(_connectedAccount);
        }
      });

      _ethereumWallet.getChainId().then((chainId) {
        print('Current chain: $chainId');
        _connectedChainId = chainId;
        _connectedChainChangedEventChannel.add(
          (_connectedChainId!, false),
        );
      });

      _ethereumWallet.getAccounts().then((accounts) {
        accounts = accounts.map((a) => convertToEip55Address(a)).toList();
        _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        print('Current account: $_connectedAccount');
        _connectedAccountChangedEventChannel.add(_connectedAccount);
      });

      l1Provider.onBlockMined((blockNumber) {
        print('Latest L1 block: $blockNumber');
        _l1BlockMinedEventChannel.add(blockNumber);
      });
    }
  }

  Future<int> getLatestL1BlockNumber() => l1Provider.getBlockNumber();

  Future<EthereumError?> switchEthereumChain() async {
    if (!available) {
      return EthereumError('Metamask not installed');
    }

    var error = await _ethereumWallet.switchChain(
      validChainId,
      'Optimism Local',
      'http://localhost:9545',
    );
    if (error != null) {
      print('Switch chain error: [${error.code}] ${error.message}');
      return EthereumError('Error trying to switch chain');
    }

    return null;
  }

  Future<EthereumError?> connectAccount() async {
    if (!available) {
      return EthereumError('Metamask not installed');
    }

    var result = await _ethereumWallet.requestAccounts();
    if (result.error != null) {
      print(
        'Request accounts error: [${result.error!.code}] ${result.error!.message}',
      );
      return EthereumError('Error requesting accounts');
    }

    return null;
  }

  Future<Either<EthereumError, String>> signThingAcceptancePollVote(
    String thingId,
    String castedAt,
    thing.DecisionIm decision,
    String reason,
  ) async {
    if (!available) {
      return Left(EthereumError('Metamask not installed'));
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

    var result = await _ethereumWallet.signTypedData(connectedAccount, data);
    if (result.error != null) {
      print(
        'Sign message error: [${result.error!.code}] ${result.error!.message}',
      );
      return Left(EthereumError('Error signing message'));
    }

    return Right(result.signature!);
  }

  Future<Either<EthereumError, String>>
      signThingSettlementProposalAssessmentPollVote(
    String thingId,
    String proposalId,
    String castedAt,
    settlement.DecisionIm decision,
    String reason,
  ) async {
    if (!available) {
      return Left(EthereumError('Metamask not installed'));
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

    var result = await _ethereumWallet.signTypedData(connectedAccount, data);
    if (result.error != null) {
      print(
        'Sign message error: [${result.error!.code}] ${result.error!.message}',
      );
      return Left(EthereumError('Error signing message'));
    }

    return Right(result.signature!);
  }

  Future<Either<EthereumError, (String, String)>> signSiweMessage(
    String account,
    String nonce,
  ) async {
    if (!available) {
      return Left(EthereumError('Metamask not installed'));
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
      hex.encode(utf8.encode(message)),
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
