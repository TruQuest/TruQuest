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
    return account.substring(0, 6) +
        '...' +
        account.substring(account.length - 4, account.length);
  }

  String? get id {
    var account = ethereumAccount;
    if (account == null) {
      return null;
    }

    account = account.length == 42 ? account.substring(2, 42) : account;
    return account.toLowerCase();
  }

  UserVm({
    required this.state,
    this.ethereumAccount,
    this.username,
  });
}
