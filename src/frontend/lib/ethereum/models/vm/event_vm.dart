class EventVm {
  final int transactionIndex;
  final int blockNumber;
  final String transactionHash;
  final String address;
  final List<String> topics;
  final String data;
  final int logIndex;
  final String blockHash;

  EventVm.fromMap(Map<String, dynamic> map)
      : transactionIndex = int.parse(map['transactionIndex']),
        blockNumber = int.parse(map['blockNumber']),
        transactionHash = map['transactionHash'],
        address = map['address'],
        // @@NOTE: Not sure why need to call 'toList' when 'cast' already returns a List, but can't use topics[index] (sub) operator without it.
        topics = (map['topics'] as List<dynamic>).cast<String>().toList(),
        data = map['data'],
        logIndex = int.parse(map['logIndex']),
        blockHash = map['blockHash'];
}
