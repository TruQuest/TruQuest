class UserVm {
  final String id;
  final String walletAddress;
  final BigInt balance;
  final BigInt stakedBalance;

  UserVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        walletAddress = map['walletAddress'],
        balance = BigInt.parse(map['hexBalance']),
        stakedBalance = BigInt.parse(map['hexStakedBalance']);
}
