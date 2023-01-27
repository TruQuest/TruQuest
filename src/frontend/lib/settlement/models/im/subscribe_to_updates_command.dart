class SubscribeToUpdatesCommand {
  final String proposalId;

  SubscribeToUpdatesCommand({required this.proposalId});

  Map<String, dynamic> toJson() => {'proposalId': proposalId};
}
