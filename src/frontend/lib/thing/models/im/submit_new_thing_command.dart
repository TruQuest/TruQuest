class SubmitNewThingCommand {
  final String thingId;

  SubmitNewThingCommand({required this.thingId});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['thingId'] = thingId;

    return map;
  }
}
