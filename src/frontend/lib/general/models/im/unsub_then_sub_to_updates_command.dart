class UnsubThenSubToUpdatesCommand {
  final String updateStreamIdentifierToUnsub;
  final String updateStreamIdentifierToSub;

  UnsubThenSubToUpdatesCommand({
    required this.updateStreamIdentifierToUnsub,
    required this.updateStreamIdentifierToSub,
  });

  Map<String, dynamic> toJson() => {
        'updateStreamIdentifierToUnsub': updateStreamIdentifierToUnsub,
        'updateStreamIdentifierToSub': updateStreamIdentifierToSub,
      };
}
