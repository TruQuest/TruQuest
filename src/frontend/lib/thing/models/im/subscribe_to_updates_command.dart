class SubscribeToUpdatesCommand {
  final String thingId;

  SubscribeToUpdatesCommand({required this.thingId});

  Map<String, dynamic> toJson() => {'thingId': thingId};
}
