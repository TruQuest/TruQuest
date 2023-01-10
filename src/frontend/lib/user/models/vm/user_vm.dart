enum UserAccountState {
  guest,
  connectedNotLoggedIn,
  connectedAndLoggedIn,
}

class UserVm {
  final UserAccountState state;
  final String? ethereumAccount;
  final String? username;

  String? get ethereumAccountShort {
    var account = ethereumAccount;
    if (account == null) {
      return null;
    }
    return account.substring(0, 4) +
        '...' +
        account.substring(account.length - 4, account.length);
  }

  UserVm({
    required this.state,
    this.ethereumAccount,
    this.username,
  });
}
