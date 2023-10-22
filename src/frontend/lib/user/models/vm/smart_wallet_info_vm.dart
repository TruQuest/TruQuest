import '../../../general/utils/utils.dart';

class SmartWalletInfoVm {
  final String? _signerAddress;
  final String? _address;
  final bool? _deployed;
  final BigInt? _ethBalance;
  final BigInt? _truBalance;
  final BigInt? _deposited;
  final BigInt? _staked;

  SmartWalletInfoVm(
    this._signerAddress,
    this._address,
    this._deployed,
    this._ethBalance,
    this._truBalance,
    this._deposited,
    this._staked,
  );

  SmartWalletInfoVm.placeholder()
      : _signerAddress = null,
        _address = null,
        _deployed = null,
        _ethBalance = null,
        _truBalance = null,
        _deposited = null,
        _staked = null;

  bool get isPlaceholder => _address == null;

  String get signerAddress => _signerAddress!;

  String get signerAddressShort {
    var address = _signerAddress;
    if (address == null) {
      return 'Not connected';
    }
    return '${address.substring(0, 6)}..${address.substring(address.length - 3)}';
  }

  String get address => _address!;

  String get addressShort {
    var address = _address;
    if (address == null) {
      return 'No address';
    }
    return '${address.substring(0, 6)}..${address.substring(address.length - 3)}';
  }

  bool get deployed => _deployed ?? false;

  String get ethBalance => '${getMinLengthAmount(_ethBalance!, 'ETH')} ETH';

  String get ethBalanceShort {
    var balance = _ethBalance;
    if (balance == null) {
      return '0 ETH';
    }

    return '${getFixedLengthAmount(balance, 'ETH')} ETH';
  }

  String get truBalance => '${getMinLengthAmount(_truBalance!, 'TRU')} TRU';

  String get truBalanceShort {
    var balance = _truBalance;
    if (balance == null) {
      return '0 TRU';
    }

    return '${getFixedLengthAmount(balance, 'TRU')} TRU';
  }

  String get depositedSlashStaked =>
      '${getMinLengthAmount(_deposited!, 'TRU')} / ${getMinLengthAmount(_staked!, 'TRU')} TRU';

  String get depositedSlashStakedShort {
    var deposited = _deposited;
    if (deposited == null) {
      return 'N/A';
    }
    return '${getFixedLengthAmount(_deposited!, 'TRU')} / ${getFixedLengthAmount(_staked!, 'TRU')} TRU';
  }
}
