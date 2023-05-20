class VerifierLotteryParticipantEntryVm {
  final int? joinedBlockNumber;
  final String userId;
  final String dataHash;
  final int? nonce;
  final bool isOrchestrator;
  final bool isWinner;

  VerifierLotteryParticipantEntryVm.fromMap(Map<String, dynamic> map)
      : joinedBlockNumber = map['joinedBlockNumber'],
        userId = map['userId'],
        dataHash = map['dataHash'],
        nonce = map['nonce'],
        isOrchestrator = map.containsKey('isOrchestrator'),
        isWinner = map.containsKey('isWinner'); // either true or absent
}
