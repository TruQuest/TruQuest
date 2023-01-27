class UnsubscribeFromUpdatesCommand {
  final String proposalId;

  UnsubscribeFromUpdatesCommand({required this.proposalId});

  Map<String, dynamic> toJson() => {'proposalId': proposalId};
}
