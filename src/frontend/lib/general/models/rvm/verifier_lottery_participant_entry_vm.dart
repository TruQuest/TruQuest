class VerifierLotteryParticipantEntryVm {
  final int? joinedBlockNumber;
  final String userId;
  final String dataHash;
  final int? nonce;

  VerifierLotteryParticipantEntryVm.fromMap(Map<String, dynamic> map)
      : joinedBlockNumber = map['joinedBlockNumber'],
        userId = map['userId'],
        dataHash = map['dataHash'],
        nonce = map['nonce'];
}
