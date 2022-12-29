import "dart:async";
import "dart:convert";

import "package:either_dart/either.dart";
import "package:flutter_web3/flutter_web3.dart";

import "../errors/ethereum_error.dart";

class EthereumService {
  final bool available;
  final int validChainId = 51234;

  int? _connectedChainId;
  int? get connectedChainId => _connectedChainId;

  String? _connectedAccount;
  String? get connectedAccount => _connectedAccount;

  final StreamController<int> _connectedChainChangedEventChannel =
      StreamController<int>();
  Stream<int> get connectedChainChanged$ =>
      _connectedChainChangedEventChannel.stream;

  final StreamController<String?> _connectedAccountChangedEventChannel =
      StreamController<String?>();
  Stream<String?> get connectedAccountChanged$ =>
      _connectedAccountChangedEventChannel.stream;

  EthereumService() : available = ethereum != null {
    var metamask = ethereum;
    if (metamask != null) {
      metamask.onChainChanged((chainId) {
        print("Chain changed: $chainId");
        if (_connectedChainId != chainId) {
          _connectedChainId = chainId;
          _connectedChainChangedEventChannel.add(chainId);
        }
      });

      metamask.onAccountsChanged((accounts) {
        print("Accounts changed: $accounts");
        _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        _connectedAccountChangedEventChannel.add(_connectedAccount);
      });

      metamask.getChainId().then((chainId) {
        print("Current chain: $chainId");
        _connectedChainId = chainId;
        _connectedChainChangedEventChannel.add(chainId);
      });

      // @@NOTE: ?? accountsChanged event doesn't fire on launch even though MM says it does ??
      metamask.getAccounts().then((accounts) {
        _connectedAccount = accounts.isNotEmpty ? accounts.first : null;
        _connectedAccountChangedEventChannel.add(_connectedAccount);
      });
    }
  }

  Future<EthereumError?> switchEthereumChain() async {
    var metamask = ethereum;
    if (metamask == null) {
      return EthereumError("Metamask not installed");
    }

    try {
      await metamask.walletSwitchChain(validChainId);
    } catch (e) {
      // catching EthereumUnrecognizedChainException doesn't work fsr
      print(e);
      try {
        await metamask.walletAddChain(
          chainId: validChainId,
          chainName: "Ganache",
          nativeCurrency: CurrencyParams(
            name: "Ether",
            symbol: "ETH",
            decimals: 18,
          ),
          rpcUrls: ["http://localhost:7545/"],
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
          {"name": "salt", "type": "bytes32"},
        ],
        "SignUpTd": [
          {"name": "username", "type": "string"},
        ],
      },
      "domain": {
        "name": "TruQuest",
        "version": "0.0.1",
        "chainId": validChainId,
        "verifyingContract": "0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c",
        "salt":
            "0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558",
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
