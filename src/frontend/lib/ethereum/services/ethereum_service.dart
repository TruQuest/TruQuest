import "dart:async";
import "dart:convert";

import "package:either_dart/either.dart";
import "package:flutter_web3/flutter_web3.dart";

import "../errors/ethereum_error.dart";

class EthereumService {
  final bool available;
  final int _validChainId = 1337;

  String? _connectedAccount;
  String? get connectedAccount => _connectedAccount;

  final StreamController<String?> _connectedAccountChangedEventChannel =
      StreamController<String?>();
  Stream<String?> get connectedAccountChanged$ =>
      _connectedAccountChangedEventChannel.stream;

  EthereumService() : available = ethereum != null {
    var metamask = ethereum;
    if (metamask != null) {
      metamask.onChainChanged((chainId) {
        print("Chain changed: $chainId");
      });

      metamask.onAccountsChanged((accounts) {
        print("Accounts changed: $accounts");
        _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        _connectedAccountChangedEventChannel.add(_connectedAccount);
      });

      // @@NOTE: ?? accountsChanged event doesn"t fire on launch even though MM says it does ??
      metamask.getAccounts().then((accounts) {
        _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        _connectedAccountChangedEventChannel.add(_connectedAccount);
      });
    }
  }

  Future<EthereumError?> connectAccount() async {
    var metamask = ethereum;
    if (metamask == null) {
      return EthereumError("Metamask not installed");
    }

    try {
      var accounts = await metamask.requestAccount();
      if (accounts.isEmpty) {
        return EthereumError("No account selected");
      }
      _connectedAccount = accounts.first;
      return null;
    } on EthereumUserRejected catch (e) {
      print(e);
      return EthereumError("User rejected the request");
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }
  }

  Future<Either<EthereumError, String>> signAuthMessage(String username) async {
    var metamask = ethereum;
    if (metamask == null) {
      return Left(EthereumError("Metamask not installed"));
    }
    if (_connectedAccount == null) {
      return Left(EthereumError("No connected account"));
    }

    Map<String, dynamic> map = {
      "types": {
        "EIP712Domain": [
          {"name": "name", "type": "string"},
          {"name": "version", "type": "string"},
          {"name": "chainId", "type": "uint256"},
          {"name": "verifyingContract", "type": "address"},
        ],
        "SignUpTd": [
          {"name": "username", "type": "string"},
        ],
      },
      "domain": {
        "name": "TruQuest",
        "version": "0.0.1",
        "chainId": 1,
        "verifyingContract": "0x1cf6f69441996615Df4370E09A2885F2448b9234",
      },
      "primaryType": "SignUpTd",
      "message": {
        "username": username,
      }
    };

    var data = jsonEncode(map);

    try {
      var signature = await metamask.request<String>(
        "eth_signTypedData_v4",
        [_connectedAccount, data],
      );

      return Right(signature);
    } catch (e) {
      print(e);
      return Left(EthereumError(e.toString()));
    }
  }
}
