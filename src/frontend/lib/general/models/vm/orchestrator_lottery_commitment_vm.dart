class OrchestratorLotteryCommitmentVm {
  final int l1BlockNumber;
  final String txnHash;
  final String dataHash;
  final String userXorDataHash;

  String get commitmentShort => dataHash.substring(0, 22) + '...';

  OrchestratorLotteryCommitmentVm.fromMap(Map<String, dynamic> map)
      : l1BlockNumber = map['l1BlockNumber'],
        txnHash = map['txnHash'],
        dataHash = map['dataHash'],
        userXorDataHash = map['userXorDataHash'];

  OrchestratorLotteryCommitmentVm.fromExportMap(Map<String, dynamic> map)
      : l1BlockNumber = map['block'],
        txnHash = '',
        dataHash = map['dataHash'],
        userXorDataHash = map['userXorDataHash'];
}
