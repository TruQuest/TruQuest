class SubscribeToUpdatesCommand {
  final String thingId;

  SubscribeToUpdatesCommand({required this.thingId});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['thingId'] = thingId;

    return map;
  }
}
