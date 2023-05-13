class UnsubscribeFromUpdatesCommand {
  final String updateStreamIdentifier;

  UnsubscribeFromUpdatesCommand({required this.updateStreamIdentifier});

  Map<String, dynamic> toJson() =>
      {'updateStreamIdentifier': updateStreamIdentifier};
}
