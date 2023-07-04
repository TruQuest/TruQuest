enum UserAccountState {
  guest,
  connectedNotLoggedIn,
  connectedAndLoggedIn,
}

class UserVm {
  final UserAccountState state;
  final String? ethereumAccount;
  final String? username;

  String? get id {
    if (state != UserAccountState.connectedAndLoggedIn) {
      return null;
    }

    var account = ethereumAccount;
    account = account!.length == 42 ? account.substring(2, 42) : account;
    return account.toLowerCase();
  }

  UserVm({
    required this.state,
    this.ethereumAccount,
    this.username,
  });
}
