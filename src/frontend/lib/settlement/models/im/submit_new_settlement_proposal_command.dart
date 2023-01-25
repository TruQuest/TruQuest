class SubmitNewSettlementProposalCommand {
  final String proposalId;

  SubmitNewSettlementProposalCommand({required this.proposalId});

  Map<String, dynamic> toJson() => {'proposalId': proposalId};
}
