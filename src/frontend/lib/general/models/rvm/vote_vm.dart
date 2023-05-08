class VoteVm {
  final String userId;
  final int? castedAtMs;
  final int? blockNumber;

  VoteVm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        castedAtMs = map['castedAtMs'],
        blockNumber = map['blockNumber'];
}
