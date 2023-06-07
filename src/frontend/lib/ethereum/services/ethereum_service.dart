import 'dart:async';
import 'dart:convert';

import 'package:convert/convert.dart';
import 'package:either_dart/either.dart';
import 'package:flutter_web3/flutter_web3.dart';
import 'package:universal_html/html.dart' as html;

import '../../thing/models/im/decision_im.dart' as thing;
import '../../settlement/models/im/decision_im.dart' as settlement;
import '../../js.dart';
import '../errors/ethereum_error.dart';

class EthereumService {
  final bool available;
  final int validChainId = 901;

  int? _connectedChainId;
  int? get connectedChainId => _connectedChainId;

  String? _connectedAccount;
  String? get connectedAccount => _connectedAccount;

  late final provider = Web3Provider(ethereum!);
  late final l1Provider = JsonRpcProvider('http://localhost:8545');

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

  EthereumService() : available = ethereum != null {
    var metamask = ethereum;
    if (metamask != null) {
      if (!isMetamaskInitialized) {
        html.window.location.reload();
      }

      // @@??: metamask.autoRefreshOnNetworkChange ?

      metamask.removeAllListeners('chainChanged');
      metamask.removeAllListeners('accountsChanged');

      metamask.onChainChanged((chainId) {
        print('Chain changed: $chainId');
        // is this redundant?
        if (_connectedChainId != chainId) {
          _connectedChainId = chainId;
          _connectedChainChangedEventChannel.add(
            (_connectedChainId!, true),
          );
        }
      });

      metamask.onAccountsChanged((accounts) {
        print('Accounts changed: $accounts');
        var connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        // is this redundant?
        if (_connectedAccount != connectedAccount) {
          _connectedAccount = connectedAccount;
          _connectedAccountChangedEventChannel.add(_connectedAccount);
        }
      });

      metamask.getChainId().then((chainId) {
        print('Current chain: $chainId');
        _connectedChainId = chainId;
        _connectedChainChangedEventChannel.add(
          (_connectedChainId!, false),
        );
      });

      // @@NOTE: ?? accountsChanged event doesn't fire on launch even though MM says it does ??
      metamask.getAccounts().then((accounts) {
        _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        print('Current account: $_connectedAccount');
        _connectedAccountChangedEventChannel.add(_connectedAccount);
      });

      l1Provider.onBlock((blockNumber) {
        print('Latest L1 block: $blockNumber');
        _l1BlockMinedEventChannel.add(blockNumber);
      });
    }
  }

  Future<int> getLatestL1BlockNumber() async {
    if (!available) {
      return 0;
    }

    return await l1Provider.getBlockNumber();
  }

  Future<EthereumError?> switchEthereumChain() async {
    var metamask = ethereum;
    if (metamask == null) {
      return EthereumError('Metamask not installed');
    }

    try {
      await metamask.walletSwitchChain(validChainId);
    } catch (e) {
      // @@TODO: Find out why catching EthereumUnrecognizedChainException doesn't work.
      print(e);
      try {
        await metamask.walletAddChain(
          chainId: validChainId,
          chainName: 'Optimism Local',
          nativeCurrency: CurrencyParams(
            name: 'Ether',
            symbol: 'ETH',
            decimals: 18,
          ),
          rpcUrls: ['http://localhost:9545/'],
        );
      } catch (e) {
        print(e);
        return EthereumError(e.toString());
      }
    }

    return null;
  }

  Future<EthereumError?> connectAccount() async {
    var metamask = ethereum;
    if (metamask == null) {
      return EthereumError('Metamask not installed');
    }

    try {
      var accounts = await metamask.requestAccount();
      if (accounts.isEmpty) {
        return EthereumError('No account selected');
      }
    } on EthereumUserRejected catch (e) {
      print(e);
      return EthereumError('User rejected the request');
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }

    return null;
  }

  Future<Either<EthereumError, String>> signThingAcceptancePollVote(
    String thingId,
    String castedAt,
    thing.DecisionIm decision,
    String reason,
  ) async {
    var metamask = ethereum;
    if (metamask == null) {
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

    try {
      var signature = await metamask.request<String>(
        'eth_signTypedData_v4',
        [connectedAccount, data],
      );

      return Right(signature);
    } catch (e) {
      print(e);
      return Left(EthereumError(e.toString()));
    }
  }

  Future<Either<EthereumError, String>>
      signThingSettlementProposalAssessmentPollVote(
    String thingId,
    String proposalId,
    String castedAt,
    settlement.DecisionIm decision,
    String reason,
  ) async {
    var metamask = ethereum;
    if (metamask == null) {
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

    try {
      var signature = await metamask.request<String>(
        'eth_signTypedData_v4',
        [connectedAccount, data],
      );

      return Right(signature);
    } catch (e) {
      print(e);
      return Left(EthereumError(e.toString()));
    }
  }

  Future<Either<EthereumError, (String, String)>> signSiweMessage(
    String account,
    String nonce,
  ) async {
    var metamask = ethereum;
    if (metamask == null) {
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

    try {
      var signature = await metamask.request<String>(
        'personal_sign',
        [hex.encode(utf8.encode(message)), account],
      );

      return Right((message, signature));
    } catch (e) {
      print(e);
      return Left(EthereumError(e.toString()));
    }
  }
}
