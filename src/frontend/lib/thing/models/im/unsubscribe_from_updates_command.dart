class UnsubscribeFromUpdatesCommand {
  final String thingId;

  UnsubscribeFromUpdatesCommand({required this.thingId});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['thingId'] = thingId;

    return map;
  }
}
