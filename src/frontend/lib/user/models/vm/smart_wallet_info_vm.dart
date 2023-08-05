import '../../../ethereum_js_interop.dart';

class SmartWalletInfoVm {
  final String? _ownerAddress;
  final String? _address;
  final bool? _deployed;
  final BigInt? _ethBalance;
  final BigInt? _truBalance;
  final BigInt? _deposited;
  final BigInt? _staked;

  SmartWalletInfoVm(
    this._ownerAddress,
    this._address,
    this._deployed,
    this._ethBalance,
    this._truBalance,
    this._deposited,
    this._staked,
  );

  SmartWalletInfoVm.placeholder()
      : _ownerAddress = null,
        _address = null,
        _deployed = null,
        _ethBalance = null,
        _truBalance = null,
        _deposited = null,
        _staked = null;

  bool get isPlaceholder => _address == null;

  String get ownerAddress => _ownerAddress!;

  String get ownerAddressShort {
    var address = _ownerAddress;
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

  String _getFixedLengthBalance(BigInt balance) {
    var balanceString = formatUnits(BigNumber.from(balance.toString()));
    var balanceStringSplit = balanceString.split('.');
    var decimals = balanceStringSplit.length == 1 ? '000' : '';
    if (decimals == '') {
      decimals = balanceStringSplit.last;
      if (decimals.length < 3) {
        decimals = decimals.padRight(3, '0');
      } else if (decimals.length > 3) {
        decimals = decimals.substring(0, 3);
      }
    }

    return '${balanceStringSplit.first}.$decimals';
  }

  String _getMinLengthBalance(BigInt balance) {
    var balanceString = formatUnits(BigNumber.from(balance.toString()));
    var balanceStringSplit = balanceString.split('.');
    var decimals = balanceStringSplit.length == 1 ? '000' : '';
    if (decimals == '') {
      decimals = balanceStringSplit.last;
      if (decimals.length < 3) {
        decimals = decimals.padRight(3, '0');
      }
    }

    return '${balanceStringSplit.first}.$decimals';
  }

  String get ethBalance => '${_getMinLengthBalance(_ethBalance!)} ETH';

  String get ethBalanceShort {
    var balance = _ethBalance;
    if (balance == null) {
      return '0 ETH';
    }

    return '${_getFixedLengthBalance(balance)} ETH';
  }

  String get truBalance => '${_getMinLengthBalance(_truBalance!)} TRU';

  String get truBalanceShort {
    var balance = _truBalance;
    if (balance == null) {
      return '0 TRU';
    }

    return '${_getFixedLengthBalance(balance)} TRU';
  }

  String get depositedSlashStaked => '${_getMinLengthBalance(_deposited!)} / ${_getMinLengthBalance(_staked!)} TRU';

  String get depositedSlashStakedShort {
    var deposited = _deposited;
    if (deposited == null) {
      return 'N/A';
    }
    return '${_getFixedLengthBalance(_deposited!)} / ${_getFixedLengthBalance(_staked!)} TRU';
  }
}
