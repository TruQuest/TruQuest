class VerifierLotteryParticipantEntryVm {
  final int l1BlockNumber;
  final String txnHash;
  final String userId;
  final String userData;
  final int? nonce;
  final bool isWinner;

  String get nonceString => nonce?.toString() ?? '*';

  String get commitment => userData.substring(0, 15) + '..';

  String get userIdShort => '${userId.substring(0, 6)}..${userId.substring(userId.length - 4)}';

  VerifierLotteryParticipantEntryVm.fromMap(Map<String, dynamic> map)
      : l1BlockNumber = map['l1BlockNumber'],
        txnHash = map['txnHash'],
        userId = map['userId'],
        userData = map['userData'],
        nonce = map['nonce'],
        isWinner = map.containsKey('isWinner'); // either true or absent
}
