class SubmitNewSettlementProposalRvm {
  final String thingId;
  final String proposalId;
  final String signature;

  SubmitNewSettlementProposalRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        proposalId = map['proposalId'],
        signature = map['signature'];
}
