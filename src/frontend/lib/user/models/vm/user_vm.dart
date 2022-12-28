enum UserAccountState {
  guest,
  connectedNotLoggedIn,
  connectedAndLoggedIn,
}

class UserVm {
  final UserAccountState state;
  final String? ethereumAccount;
  final String? username;

  UserVm({
    required this.state,
    this.ethereumAccount,
    this.username,
  });
}
