class UnsubscribeFromUpdatesCommand {
  final String thingId;

  UnsubscribeFromUpdatesCommand({required this.thingId});

  Map<String, dynamic> toJson() => {'thingId': thingId};
}
