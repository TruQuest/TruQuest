class UserVm {
  final bool isGuest;
  final String? walletAddress;
  final String? username;

  String? get id {
    if (isGuest) {
      return null;
    }

    var account = walletAddress;
    account = account!.length == 42 ? account.substring(2, 42) : account;
    return account.toLowerCase();
  }

  UserVm({
    required this.isGuest,
    this.walletAddress,
    this.username,
  });
}
