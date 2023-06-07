class VerifierLotteryParticipantEntryVm {
  final int joinedBlockNumber;
  final String userId;
  final String? userData;
  final int? nonce;
  final bool isOrchestrator;
  final bool isWinner;

  VerifierLotteryParticipantEntryVm._({
    required this.joinedBlockNumber,
    required this.userId,
    required this.userData,
    required this.nonce,
    required this.isOrchestrator,
    required this.isWinner,
  });

  VerifierLotteryParticipantEntryVm.fromMap(Map<String, dynamic> map)
      : joinedBlockNumber = map['joinedBlockNumber'],
        userId = map['userId'],
        userData = map['userData'],
        nonce = map['nonce'],
        isOrchestrator = map.containsKey('isOrchestrator'),
        isWinner = map.containsKey('isWinner'); // either true or absent

  VerifierLotteryParticipantEntryVm.orchestratorNoNonce(
    this.joinedBlockNumber,
    String dataHash,
    String userXorDataHash,
  )   : userId = 'Orchestrator',
        userData = '$dataHash|$userXorDataHash',
        nonce = null,
        isOrchestrator = true,
        isWinner = false;

  VerifierLotteryParticipantEntryVm copyWith(
    String userId,
    String dataHash,
    String userXorDataHash,
  ) {
    return VerifierLotteryParticipantEntryVm._(
      joinedBlockNumber: joinedBlockNumber,
      userId: userId,
      userData: '$dataHash|$userXorDataHash',
      nonce: nonce,
      isOrchestrator: isOrchestrator,
      isWinner: isWinner,
    );
  }
}
