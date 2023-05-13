class SubscribeToUpdatesCommand {
  final String updateStreamIdentifier;

  SubscribeToUpdatesCommand({required this.updateStreamIdentifier});

  Map<String, dynamic> toJson() =>
      {'updateStreamIdentifier': updateStreamIdentifier};
}
