import '../../../general/extensions/datetime_extension.dart';

class VoteVm {
  final String userId;
  final int? castedAtMs;
  final int? blockNumber;

  VoteVm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        castedAtMs = map['castedAtMs'],
        blockNumber = map['blockNumber'];

  String get castedOffChainAt => castedAtMs != null
      ? DateTime.fromMillisecondsSinceEpoch(castedAtMs!).getString()
      : '–';

  String get castedOnChainAtBlockNo => blockNumber?.toString() ?? '–';
}
